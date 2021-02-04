using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;
using lox.monads;

namespace lox.runtime
{
    using Result = lox.monads.Result<object, RuntimeError>;
    public record RuntimeError(Token token, string message);

    public class Interpreter : ExprVisitor<Result>, StmtVisitor<Result>
    {
        public Env Globals { get; } = new();
        private Env Env;
        
        //helper for emulating Result<void,error>
        private object Void => null;

        private class Clock : Callable
        {
            public int Arity => 0;
            public Result<object, RuntimeError> Call(Interpreter interpreter, IEnumerable<object> args) =>
                Result.Ok((double)DateTimeOffset.Now.ToUnixTimeMilliseconds()/1000);
            public override string ToString() =>
                "<native fn: clock>";
        }

        public Interpreter()
        {
            Env = Globals;
            Globals.Define("clock", new Clock());
        }

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
                TokenType.LessEqual => evalNumeric((l,r) => l <= r),
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

        public Result VisitCallExpr(CallExpr expr) =>
            from callee in
                Eval(expr.callee)
            from callable in
                callee is Callable c
                ? Result<Callable,RuntimeError>.Ok(c)
                : Result<Callable,RuntimeError>.Err(new RuntimeError(expr.paren, "Can only call functions and classes."))
            from args in
                expr
                .arguments
                .Select(Eval)
                .ToResult()
            from function in
                args.Count() == callable.Arity
                ? Result<Callable,RuntimeError>.Ok(callable)
                : Result<Callable,RuntimeError>.Err(new RuntimeError(expr.paren, "Can only call functions and classes."))
            from result in
                function.Call(this, args)
            select result;

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
            Result<bool,RuntimeError> Predicate(Token @operator, bool isTruthy) =>
                @operator.TokenType switch {
                    TokenType.And => Result<bool,RuntimeError>.Ok(!isTruthy),
                    TokenType.Or => Result<bool,RuntimeError>.Ok(isTruthy),
                    _ => Result<bool,RuntimeError>.Err(new RuntimeError(@operator, "Expected logical operator."))
                };

            return
                from left in Eval(expr.left)
                from condition in Result<bool,RuntimeError>.Ok(IsTruthy(left))
                // translate into a consistent boolean value regardless of if we're talking about AND or OR
                from predicate in Predicate(expr.@operator, condition)
                from right in predicate ? Result.Ok(left) : Eval(expr.right)
                select right;
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

        internal Result ExecuteBlock(List<Stmt> statements, Env env)
        {
            // it's a little crude to modify the internal state like this
            // the alternative is to pass the env along and provide an immutable copy to the functions
            // I know we use result types, but we use exceptions to `return`,
            // so we must wrap the execution in a try/finally to ensure we restore the environment.
            var prev = this.Env;
            this.Env = env;
            try
            {
                return
                    statements
                    .Aggregate(
                        Result<object,RuntimeError>.Ok(null),
                        (acc,curr) => acc.Bind(_ => Execute(curr))
                    );
            }
            finally
            {
                this.Env = prev;
            }
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
            Env.Define(stmt.name.Lexeme, new Function(stmt));
            return Result.Ok(Void);
        }


        public Result VisitIfStmt(IfStmt stmt) =>
            from condition in EvalCondition(stmt.condition)
            from result in ExecuteIfStmt(condition, stmt.thenBranch, stmt.elseBranch)
            select result;

        private Result ExecuteIfStmt(bool predicate, Stmt thenBranch, Stmt elseBranch)
        {
            if (predicate)
                return Execute(thenBranch);
            else if(elseBranch is not null)
                return Execute(elseBranch);
            else
                return Result.Ok(Void);
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
            var result =
                from value in stmt is null ? Result.Ok(null) : Eval(stmt.value)
                select value;
            // an interesting, if not questionable, hack to immediately short circuit
            // the call stack and return directly to the caller
            return
                result.Bind<object>(v => throw new Return(v));
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

        public Result VisitWhileStmt(WhileStmt stmt)
        {
            // this is what the code below does,
            //
            // while(IsTruthy(Eval(stmt.condition)))
            // {
            //     Execute(stmt.body);
            // }
            // return null;
            // 
            // but we need to handle and propogate RuntimeErrors
            // so we need to evaluate the condition inside the loop
            // if an error occurs, we stop
            // if the condition evaluates to false, we stop
            // 
            // Because the okSelector for MapOrElse returns a plain value,
            // instead of a monad, we have to unwrap the underlying values.
            //
            // Recursion might be an option, but C# doesn't do tail call optimization,
            // So doing so would likely blow the stack

            var condition = true;
            Result result = null;
            while (condition)
            {
                result = 
                    EvalCondition(stmt.condition)
                    .MapOrElse(c => {
                        condition = c;
                        if(c)
                        {
                            var r = Execute(stmt.body);
                            if (r.IsErr())
                                return r.Error();
                            return r.Unwrap();
                        }
                        else
                        {
                            return Void;
                        }
                    }, err => {
                        condition = false;
                        return err;
                    });
            }
            return result;
        }

        private Result<bool, RuntimeError> EvalCondition(Expr condition) =>
            from c in Eval(condition)
            select IsTruthy(c);
    }
}