using System.Collections.Generic;

namespace lox.ast {

    //TODO: The visitor patter really just makes up for a lack of pattern matching.
    //In C# we can use pattern matching to do away with this I think.
    public interface ExprVisitor<T>
    {
        T VisitAssignExpr(AssignExpr expr);
        T VisitBinaryExpr(BinaryExpr expr);
        T VisitCallExpr(CallExpr expr);
        T VisitGetExpr(GetExpr expr);
        T VisitGroupingExpr(GroupingExpr expr);
        T VisitLiteralExpr(LiteralExpr expr);
        T VisitLogicalExpr(LogicalExpr expr);
        T VisitSetExpr(SetExpr expr);
        T VisitSuperExpr(SuperExpr expr);
        T VisitThisExpr(ThisExpr expr);
        T VisitUnaryExpr(UnaryExpr expr);
        T VisitVariableExpr(VariableExpr expr);
    }
    public abstract record Expr
    {
        public abstract T Accept<T>(ExprVisitor<T> visitor);
    }
    public record AssignExpr(Token name, Expr value) : Expr {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitAssignExpr(this);
    }
    public record BinaryExpr(Expr left, Token @operator, Expr right) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitBinaryExpr(this);
    }
    public record CallExpr(Expr callee, Token paren, IEnumerable<Expr> arguments) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitCallExpr(this);
    }
    public record GetExpr(Expr @object, Token name) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) => 
            visitor.VisitGetExpr(this);
    }
    public record GroupingExpr(Expr expr) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitGroupingExpr(this);
    }

    public record LiteralExpr(object value) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitLiteralExpr(this);
    }
    public record LogicalExpr(Expr left, Token @operator, Expr right) : Expr
    {
        // this is a binary op, could we just express it as one???
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitLogicalExpr(this);
    }
    public record SetExpr(Expr @object, Token name, Expr value) : Expr
    {
        // also a binary op
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitSetExpr(this);
    }
    public record SuperExpr(Token keyword, Token method) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitSuperExpr(this);
    }
    public record ThisExpr(Token keyword) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitThisExpr(this);
    }
    public record UnaryExpr(Token @operator, Expr right) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitUnaryExpr(this);
    }
    public record VariableExpr(Token name) : Expr
    {
        public override T Accept<T>(ExprVisitor<T> visitor) =>
            visitor.VisitVariableExpr(this);
    }

    public interface StmtVisitor<T>
    {
        T VisitBlockStmt(BlockStmt blockStmt);
        T VisitClassStmt(ClassStmt classStmt);
        T VisitExpressionStmt(ExpressionStmt expressionStmt);
        T VisitFunctionStmt(FunctionStmt functionStmt);
        T IfStmt(IfStmt ifStmt);
        T PrintStmt(PrintStmt printStmt);
        T ReturnStmt(ReturnStmt returnStmt);
        T VarStmt(VarStmt varStmt);
        T WhileStmt(WhileStmt whileStmt);
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
            visitor.IfStmt(this);
    }

    public record PrintStmt(Expr expr) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.PrintStmt(this);
    }

    public record ReturnStmt(Token keyword, Expr value) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.ReturnStmt(this);
    }

    public record VarStmt(Token name, Expr initializer) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.VarStmt(this);
    }

    public record WhileStmt(Expr condition, Stmt body) : Stmt
    {
        public override T Accept<T>(StmtVisitor<T> visitor) =>
            visitor.WhileStmt(this);
    }
}