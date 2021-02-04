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
            {"unary", 0},
            {"print", 0},
            {"var", 0},
            {"varDecl", 0},
            {"call", 0},
            {"function", 0},
            {"return", 0},
            {"if", 0},
            {"assignment", 0}
        };

        private readonly List<string> Labels = new();

        public string Print(List<Stmt> statements)
        {
            var graph =
                statements
                .Select(s => s.Accept(this))
                .Aggregate((acc,cur) => $"{acc}{Environment.NewLine}{cur}");

            // var graph = expr.Accept(this);
            var labels = Labels.Aggregate((acc,cur) => $"{acc}{Environment.NewLine}{cur}");
            return
                $"digraph {{{Environment.NewLine}{labels}{Environment.NewLine}{graph}{Environment.NewLine}}}";
        }

        public string Format(string nodeName, Expr expr)
        {
            return $"\t{nodeName} -> {expr.Accept(this)}";
        }

        public string Format(string nodeName, Expr left, Expr right)
        {
            return $"\t{nodeName} -> {left.Accept(this)}{Environment.NewLine}\t{nodeName} -> {right.Accept(this)}";
        }

        public string FormatLabel(string nodeName, string nodeLabel) =>
            $"\t{nodeName} [label = \"{nodeLabel}\"]";

        public string VisitAssignExpr(AssignExpr expr)
        {
            var nodeName = NodeName("assignment");
            Labels.Add(FormatLabel(nodeName, $"{expr.name.Lexeme} ="));
            return Format(nodeName, expr.value);
        }

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            var nodeName = NodeName("binop");
            Labels.Add(FormatLabel(nodeName, expr.@operator.Lexeme));
            return Format(nodeName, expr.left, expr.right);
        }

        public string VisitCallExpr(CallExpr expr)
        {
            var nodeName = NodeName("call");
            Labels.Add(FormatLabel(nodeName, "call"));
            var callee = expr.callee.Accept(this);

            var args =
                expr.arguments
                .Select(a => a.Accept(this))
                .Select(a => $"\t{callee} -> {a}{Environment.NewLine}")
                .Aggregate(String.Empty, (acc,cur) => $"{acc}{cur}");

            return $"{nodeName} -> {callee}{Environment.NewLine}{args}";
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
            var nodeName = NodeName("var");
            Labels.Add(FormatLabel(nodeName, expr.name.Lexeme));
            return nodeName;
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
            return stmt.expr.Accept(this);
        }

        public string VisitFunctionStmt(FunctionStmt stmt)
        {
            var nodeName = NodeName("function");
            Labels.Add(FormatLabel(nodeName, $"fn {stmt.name.Lexeme}"));
            return 
                stmt.body
                .Select(a => a.Accept(this))
                .Select(stmt => $"\t{nodeName} -> {stmt}")
                .Aggregate((acc,cur) => $"{acc}{Environment.NewLine}{cur}");
        }

        public string VisitIfStmt(IfStmt stmt)
        {
            var nodeName = NodeName("if");
            Labels.Add(FormatLabel(nodeName, "if"));

            var condition = stmt.condition.Accept(this);
            var thenBranch = stmt.thenBranch.Accept(this);
            if (stmt.elseBranch is null)
                return $"\t{nodeName} -> {condition}{Environment.NewLine}\t{nodeName} -> {thenBranch}";
            var elseBranch = stmt.elseBranch.Accept(this);
            return $"\t{nodeName} -> {condition}{Environment.NewLine}\t{nodeName} -> {thenBranch}{Environment.NewLine}\t{nodeName} -> {elseBranch}";
        }

        public string VisitPrintStmt(PrintStmt stmt)
        {
            var nodeName = NodeName("print");
            Labels.Add(FormatLabel(nodeName, "print"));
            return Format(nodeName, stmt.expr);
        }

        public string VisitReturnStmt(ReturnStmt stmt)
        {
            var nodeName = NodeName("return");
            Labels.Add(FormatLabel(nodeName, "return"));
            if (stmt.value is null)
                return nodeName;
            return Format(nodeName, stmt.value);
        }

        public string VisitVarStmt(VarStmt stmt)
        {
            var nodeName = NodeName("varDecl");
            Labels.Add(FormatLabel(nodeName, $"var {stmt.name.Lexeme} ="));
            return Format(nodeName, stmt.initializer);
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