using System;
using System.Linq;
using System.Collections.Generic;
using lox.ast;

namespace lox.tools
{
    ///<Summary>Prints a graphviz dot representation of the AST</Summary>
    public class AstPrinter : ExprVisitor<string>, StmtVisitor<string>
    {
        private readonly Dictionary<string, int> Counters = new Dictionary<string, int>()
        {
            {"binop", 0},
            {"group", 0},
            {"literal", 0},
            {"unary", 0}
        };

        private readonly List<string> Labels = new();

        public string Print(Expr expr)
        {
            var graph = expr.Accept(this);
            var labels = Labels.Aggregate((acc,cur) => $"{acc}{Environment.NewLine}{cur}");
            return
                $"digraph {{{Environment.NewLine}{labels}{Environment.NewLine}{Environment.NewLine}{graph}{Environment.NewLine}}}";
        }
            

        public string Format(string nodeName, Expr expr)
        {
            return $"{nodeName} -> {expr.Accept(this)}";
        }

        public string Format(string nodeName, Expr left, Expr right)
        {
            return $"\t{nodeName} -> {left.Accept(this)}{Environment.NewLine}\t{nodeName} -> {right.Accept(this)}";
        }

        public string FormatLabel(string nodeName, string nodeLabel) =>
            $"\t{nodeName} [label = \"{nodeLabel}\"]";

        public string VisitAssignExpr(AssignExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            var nodeName = NodeName("binop");
            Labels.Add(FormatLabel(nodeName, expr.@operator.Lexeme));
            return Format(nodeName, expr.left, expr.right);
        }

        public string VisitCallExpr(CallExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitGetExpr(GetExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitGroupingExpr(GroupingExpr expr)
        {
            var nodeName = NodeName("group");
            Labels.Add(FormatLabel(nodeName, "group"));
            return Format(nodeName, expr.expr);
        }

        public string VisitLiteralExpr(LiteralExpr expr)
        {
            var nodeName = NodeName("literal");
            Labels.Add(FormatLabel(nodeName, expr.value?.ToString() ?? "nil"));
            return nodeName;
        }

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

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            var nodeName = NodeName("unary");
            Labels.Add(FormatLabel(nodeName, expr.@operator.Lexeme));
            return Format(nodeName, expr.right);
        }

        public string VisitVariableExpr(VariableExpr expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitBlockStmt(BlockStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitClassStmt(ClassStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitExpressionStmt(ExpressionStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitFunctionStmt(FunctionStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitIfStmt(IfStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitPrintStmt(PrintStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitReturnStmt(ReturnStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitVarStmt(VarStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitWhileStmt(WhileStmt stmt)
        {
            throw new NotImplementedException();
        }

        private string NodeName(string nodeType)
        {
            Counters[nodeType] += 1;
            return $"{nodeType}_{Counters[nodeType]}";
        }
    }
}