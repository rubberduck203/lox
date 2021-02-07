using System;
using System.Linq;
using System.Collections.Generic;
using lox.ast;

namespace lox.runtime
{
    using Result = lox.monads.Result<object, RuntimeError>;
    public class Resolver : StmtVisitor<Result>, ExprVisitor<Result>
    {
        private readonly object Void = null;
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string,bool>> scopes = new();

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public Result Resolve(List<Stmt> statements)
        {
            //FIXME: actually handle errors
            foreach(var stmt in statements)
            {
                Resolve(stmt);
            }
            return Result.Ok(Void);
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            this.scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            var _ = this.scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (!this.scopes.Any())
                return;

            var scope = this.scopes.Peek();
            scope[name.Lexeme] = false;
        }

        private void Define(Token name)
        {
            if (!this.scopes.Any())
                return;

            var scope = this.scopes.Peek();
            scope[name.Lexeme] = true;
        }

        public Result VisitAssignExpr(AssignExpr expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return Result.Ok(Void);
        }

        public Result VisitBinaryExpr(BinaryExpr expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return Result.Ok(Void);
        }

        public Result VisitBlockStmt(BlockStmt stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return Result.Ok(Void);
        }

        public Result VisitCallExpr(CallExpr expr)
        {
            Resolve(expr.callee);
            foreach(var arg in expr.arguments)
            {
                Resolve(arg);
            }
            return Result.Ok(Void);
        }

        public Result VisitClassStmt(ClassStmt stmt)
        {
            throw new System.NotImplementedException();
        }

        public Result VisitExpressionStmt(ExpressionStmt stmt)
        {
            Resolve(stmt.expr);
            return Result.Ok(Void);
        }

        public Result VisitFunctionStmt(FunctionStmt stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt);
            return Result.Ok(Void);
        }

        private void ResolveFunction(FunctionStmt function) {
            BeginScope();
            foreach(var parameter in function.parameters)
            {
                Declare(parameter);
                Define(parameter);
            }
            Resolve(function.body);
            EndScope();
        }

        public Result VisitGetExpr(GetExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public Result VisitGroupingExpr(GroupingExpr expr)
        {
            Resolve(expr.expr);
            return Result.Ok(Void);
        }

        public Result VisitIfStmt(IfStmt stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch is not null)
                Resolve(stmt.elseBranch);
            return Result.Ok(Void);
        }

        public Result VisitLiteralExpr(LiteralExpr expr)
        {
            //literal is a leaf of the tree
            return Result.Ok(Void);
        }

        public Result VisitLogicalExpr(LogicalExpr expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return Result.Ok(Void);
        }

        public Result VisitPrintStmt(PrintStmt stmt)
        {
            Resolve(stmt.expr);
            return Result.Ok(Void);
        }

        public Result VisitReturnStmt(ReturnStmt stmt)
        {
            if (stmt.value is not null)
                Resolve(stmt.value);
            return Result.Ok(Void);
        }

        public Result VisitSetExpr(SetExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public Result VisitSuperExpr(SuperExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public Result VisitThisExpr(ThisExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public Result VisitUnaryExpr(UnaryExpr expr)
        {
            Resolve(expr.right);
            return Result.Ok(Void);
        }

        public Result VisitVariableExpr(VariableExpr expr)
        {
            if (this.scopes.Any())
            {
                if (this.scopes.Peek().TryGetValue(expr.name.Lexeme, out var defined))
                {
                    if (!defined)
                        return Result.Err(new RuntimeError(expr.name, "Can't read local variable in its own initializer."));
                }
            }

            ResolveLocal(expr, expr.name);
            return Result.Ok(Void);
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for(int i = this.scopes.Count - 1; i >= 0; i--)
            {
                if (this.scopes.ElementAt(i).ContainsKey(name.Lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count -1 - i);
                    return;
                }
            }
        }

        public Result VisitVarStmt(VarStmt stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer is not null) {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return Result.Ok(Void);
        }

        public Result VisitWhileStmt(WhileStmt stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return Result.Ok(Void);
        }
    }
}