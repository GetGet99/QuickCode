namespace QuickCode.AST.Expressions.Values;

public record class ListDeclarationAST(ListAST<ExpressionAST> Elements) : ValueAST;
