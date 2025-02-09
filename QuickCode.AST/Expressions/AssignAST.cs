namespace QuickCode.AST.Expressions;

public record class AssignAST(IdentifierAST Left, ExpressionAST Right) : ExpressionAST;
