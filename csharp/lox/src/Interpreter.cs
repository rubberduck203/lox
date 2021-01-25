using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;

namespace lox
{
    public class Interpreter : Visitor<object>
    {
        public object VisitAssignExpr(AssignExpr expr)
        {
            throw new NotImplementedException();
        }

        public object VisitBinaryExpr(BinaryExpr expr)
        {
            var left = Evaluate(expr.left);
            var right = Evaluate(expr.right);

            return expr.@operator.TokenType switch {
                TokenType.Greater => (double)left > (double)right,
                TokenType.GreaterEqual => (double)left >= (double)right,
                TokenType.Less => (double)left < (double)right,
                TokenType.LessEqual => (double)left <= (double)right,
                TokenType.BangEqual => !IsEqual(left, right),
                TokenType.EqualEqual => IsEqual(left, right),
                TokenType.Minus => (double)left - (double)right,
                TokenType.Plus  => EvalAddition(left, right),
                TokenType.Slash => (double)left / (double)right,
                TokenType.Star  => (double)left * (double)right,
                _ => throw new InvalidOperationException($"Invalid binary operation: {expr}")
            };
        }

        private object EvalAddition(object left, object right) =>
            (left,right) switch {
                (Double l, Double r) => l + r,
                (String l, String r) => l + r,
                _ => throw new InvalidOperationException($"Type error: Cannot add {left} and {right}.")
            };

        public object VisitCallExpr(CallExpr expr)
        {
            throw new NotImplementedException();
        }

        public object VisitGetExpr(GetExpr expr)
        {
            throw new NotImplementedException();
        }

        public object VisitGroupingExpr(GroupingExpr expr) =>
            Evaluate(expr.expr);

        public object VisitLiteralExpr(LiteralExpr expr) =>
            expr.value;

        public object VisitLogicalExpr(LogicalExpr expr)
        {
            throw new NotImplementedException();
        }

        public object VisitSetExpr(SetExpr expr)
        {
            throw new NotImplementedException();
        }

        public object VisitSuperExpr(SuperExpr expr)
        {
            throw new NotImplementedException();
        }

        public object VisitThisExpr(ThisExpr expr)
        {
            throw new NotImplementedException();
        }

        public object VisitUnaryExpr(UnaryExpr expr)
        {
            var right = Evaluate(expr.right);
            return expr.@operator.TokenType switch {
                TokenType.Bang => !IsTruthy(right),
                TokenType.Minus => -(double)right,
                _ => throw new InvalidOperationException("Invalid unary operator.")
            };
        }

        public object VisitVariableExpr(VariableExpr expr)
        {
            throw new NotImplementedException();
        }

        private object Evaluate(Expr expr) =>
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