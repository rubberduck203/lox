using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;
using lox.monads;

namespace lox
{
    using Result = lox.monads.Result<object, RuntimeError>;
    public record RuntimeError(Token token, string message);

    public class Interpreter : ExprVisitor<Result>, StmtVisitor<Result>
    {
        private Env Env = new();
        
        //helper for emulating Result<void,error>
        private object Void => null;

        public IEnumerable<RuntimeError> Interpret(List<Stmt> statements) =>
            statements
            .Select(Execute)
            .ToList() //ensure we materialize all stmt executions
            .Where(r => r.IsErr())
            .Select(r => r.Error());

        private Result Execute(Stmt stmt) =>
            stmt.Accept(this);

        public Result VisitAssignExpr(AssignExpr expr) =>
            from value in Eval(expr.value)
            from _ in Env.Assign(expr.name, value)
            select value;

        public Result VisitBinaryExpr(BinaryExpr expr) =>
            from left in Eval(expr.left)
            from right in Eval(expr.right)
            from result in EvalBinaryExpr(expr.@operator, left, right)
            select result;

        private Result EvalBinaryExpr(Token token, object left, object right)
        {
            var evalNumeric = EvalNumericOperation(token, left, right);

            return token.TokenType switch {
                TokenType.Greater => evalNumeric((l,r) => l > r),
                TokenType.GreaterEqual => evalNumeric((l,r)  => l >=r),
                TokenType.Less => evalNumeric((l,r) => l < r),
                TokenType.LessEqual => evalNumeric((l,r) => l < r),
                TokenType.BangEqual => Result.Ok(!IsEqual(left, right)),
                TokenType.EqualEqual => Result.Ok(IsEqual(left, right)),
                TokenType.Minus => evalNumeric((l,r) => l - r),
                TokenType.Plus  => evalNumeric((l,r) => l + r),
                TokenType.Slash => evalNumeric((l,r) => l / r),
                TokenType.Star  => evalNumeric((l,r) => l * r),
                _ => Result.Err(new RuntimeError(token, $"Invalid binary operation."))
            };
        }

        private Func<Func<Double, Double, object>,Result> EvalNumericOperation(Token token, object left, object right) =>
            f => (left, right) switch {
                (Double l, Double r) => Result.Ok(f(l,r)),
                _ => Result.Err(new RuntimeError(token, $"Type error: Cannot perform operation on {left} and {right}"))
            };

        public Result VisitCallExpr(CallExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitGetExpr(GetExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitGroupingExpr(GroupingExpr expr) =>
            Eval(expr.expr);

        public Result VisitLiteralExpr(LiteralExpr expr) =>
            Result.Ok(expr.value);

        public Result VisitLogicalExpr(LogicalExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitSetExpr(SetExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitSuperExpr(SuperExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitThisExpr(ThisExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitUnaryExpr(UnaryExpr expr) =>
            from right in Eval(expr.right)
            from result in EvalUnary(expr.@operator, right) 
            select result;

        private Result<object, RuntimeError> EvalUnary(Token token, object right) =>
            token.TokenType switch {
                TokenType.Bang => Result.Ok(!IsTruthy(right)),
                TokenType.Minus => right switch
                {
                    Double r => Result.Ok(-r),
                    _ => Result.Err(new RuntimeError(token, "Invalid unary operation."))
                },
                _ => Result.Err(new RuntimeError(token, "Invalid unary operation."))
            };

        public Result VisitVariableExpr(VariableExpr expr) =>
            Env.Lookup(expr.name);

        private Result Eval(Expr expr) =>
            expr.Accept(this);

        private bool IsTruthy(object value) =>
            value switch {
                Boolean b => b,
                null => false,
                _ => true
            };

        private bool IsEqual(object left, object right) =>
            (left,right) switch {
                (null,null) => true,
                (null,_) => false,
                (object a, object b) => a.Equals(b) 
            };

        public Result VisitBlockStmt(BlockStmt stmt) =>
            ExecuteBlock(stmt.statements, new Env(this.Env));

        private Result ExecuteBlock(List<Stmt> statements, Env env)
        {
            // it's a little crude to modify the internal state like this
            // the alternative is to pass the env along and provide an immutable copy to the functions
            var prev = this.Env;
            this.Env = env;

            var result =
                statements
                .Aggregate(
                    Result<object,RuntimeError>.Ok(null),
                    (acc,curr) => acc.Bind(_ => Execute(curr))
                );

            this.Env = prev;
            return result;
        }

        public Result VisitClassStmt(ClassStmt stmt)
        {
            throw new NotImplementedException();
        }

        public Result VisitExpressionStmt(ExpressionStmt stmt) =>
            // expressions may have side effects, 
            // so we have to evaluate it, even though we're discarding it
            from value in Eval(stmt.expr)
            select Void;

        public Result VisitFunctionStmt(FunctionStmt stmt)
        {
            throw new NotImplementedException();
        }

        public Result VisitIfStmt(IfStmt stmt) =>
            from condition in Eval(stmt.condition)
            from predicate in Result<bool, RuntimeError>.Ok(IsTruthy(condition))
            from result in ExecuteIfStmt(predicate, stmt.thenBranch, stmt.elseBranch)
            select result;

        private Result ExecuteIfStmt(bool predicate, Stmt thenBranch, Stmt elseBranch)
        {
            if (predicate)
                return Execute(thenBranch);
            else if(elseBranch is not null)
                return Execute(elseBranch);
            else
                return Result.Ok(null);
        }

        public Result<object, RuntimeError> VisitPrintStmt(PrintStmt stmt) => 
            // method syntax makes it easier to perform side effects
            Eval(stmt.expr)
            .Select(value => {
                Console.WriteLine(value);
                return Void;
            });

        public Result<object, RuntimeError> VisitReturnStmt(ReturnStmt stmt)
        {
            throw new NotImplementedException();
        }

        public Result<object, RuntimeError> VisitVarStmt(VarStmt stmt)
        {
            var value =
                stmt.initializer is null
                ? Result.Ok(null)
                : Eval(stmt.initializer);
            
            return 
                value
                .Select(value => {
                    Env.Define(stmt.name.Lexeme, value);
                    return Void;
                });
        }

        public Result<object, RuntimeError> VisitWhileStmt(WhileStmt stmt)
        {
            throw new NotImplementedException();
        }
    }
}