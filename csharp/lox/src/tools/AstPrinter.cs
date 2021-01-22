using System;
using lox.ast;

namespace lox.tools
{
    ///<Summary>Prints a graphviz dot representation of the AST</Summary>
    public class AstPrinter : Visitor<string>
    {
        public string Print(Expr expr) =>
            $"digraph {{{Environment.NewLine} {expr.Accept(this)}{Environment.NewLine}}}";

        public string Format(string nodeName, Expr expr) =>
            $"\"{nodeName}\" -> {expr.Accept(this)}";

        public string Format(string nodeName, Expr left, Expr right) =>
$@"""{nodeName}"" -> {left.Accept(this)}
 ""{nodeName}"" -> {right.Accept(this)}";

        public string VisitAssignExpr(AssignExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitBinaryExpr(BinaryExpr expr) =>
            Format(expr.@operator.Lexeme, expr.left, expr.right);

        public string VisitCallExpr(CallExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitGetExpr(GetExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitGroupingExpr(GroupingExpr expr) =>
            Format("group", expr.expr);

        public string VisitLiteralExpr(LiteralExpr expr) =>
            expr.value?.ToString() ?? "nil"; 

        public string VisitLogicalExpr(LogicalExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitSetExpr(SetExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitSuperExpr(SuperExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitThisExpr(ThisExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitUnaryExpr(UnaryExpr expr) =>
            Format(expr.@operator.Lexeme, expr.right);

        public string VisitVariableExpr(VariableExpr expr)
        {
            throw new System.NotImplementedException();
        }
    }
}