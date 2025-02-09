namespace QuickCode.AST.Expressions.Values;

public record class ArrayDeclarationWithValuesAST(ListAST<ExpressionAST> Elements) : ValueAST;