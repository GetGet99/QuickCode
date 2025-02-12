namespace QuickCode.AST.Expressions;

public record class MemberExpressionAST(ExpressionAST Expression, IdentifierAST Member) : ExpressionAST;