using System.Collections.Generic;

namespace lox.ast {

    //TODO: The visitor patter really just makes up for a lack of pattern matching.
    //In C# we can use pattern matching to do away with this I think.
    public interface StmtVisitor<T>
    {
        T VisitBlockStmt(BlockStmt stmt);
        T VisitClassStmt(ClassStmt stmt);
        T VisitExpressionStmt(ExpressionStmt stmt);
        T VisitFunctionStmt(FunctionStmt stmt);
        T VisitIfStmt(IfStmt stmt);
        T VisitPrintStmt(PrintStmt stmt);
        T VisitReturnStmt(ReturnStmt stmt);
        T VisitVarStmt(VarStmt stmt);
        T VisitWhileStmt(WhileStmt stmt);
    }

    public abstract record Stmt
    {
        public abstract T Accept<T>(StmtVisitor<T> visitor);
    }

    public record BlockStmt(List<Stmt> statements) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitBlockStmt(this);
    }

    public record ClassStmt(Token name, VariableExpr superClass, List<FunctionStmt> methods) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitClassStmt(this);
    }

    public record ExpressionStmt(Expr expr) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitExpressionStmt(this);
    }

    public record FunctionStmt(Token name, List<Token> parameters, List<Stmt> body) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitFunctionStmt(this);
    }

    public record IfStmt(Expr condition, Stmt thenBranch, Stmt elseBranch) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitIfStmt(this);
    }

    public record PrintStmt(Expr expr) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitPrintStmt(this);
    }

    public record ReturnStmt(Token keyword, Expr value) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitReturnStmt(this);
    }

    public record VarStmt(Token name, Expr initializer) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitVarStmt(this);
    }

    public record WhileStmt(Expr condition, Stmt body) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VisitWhileStmt(this);
    }
}