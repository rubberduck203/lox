using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;
using lox.monads;

namespace lox
{
    using Result = lox.monads.Result<object, RuntimeError>;
    public record RuntimeError(Token token, string message);

    public class Interpreter : Visitor<Result>
    {

        public Result VisitAssignExpr(AssignExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitBinaryExpr(BinaryExpr expr) =>
            from left in Evaluate(expr.left)
            from right in Evaluate(expr.right)
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

        Func<Func<Double, Double, object>,Result> EvalNumericOperation(Token token, object left, object right)
        {
            return f => (left, right) switch {
                (Double l, Double r) => Result.Ok(f(l,r)),
                _ => Result.Err(new RuntimeError(token, $"Type error: Cannot perform operation on {left} and {right}"))
            };
        }

        public Result VisitCallExpr(CallExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitGetExpr(GetExpr expr)
        {
            throw new NotImplementedException();
        }

        public Result VisitGroupingExpr(GroupingExpr expr) =>
            Evaluate(expr.expr);

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
            from right in Evaluate(expr.right)
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

        public Result VisitVariableExpr(VariableExpr expr)
        {
            throw new NotImplementedException();
        }

        private Result Evaluate(Expr expr) =>
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
    }
}