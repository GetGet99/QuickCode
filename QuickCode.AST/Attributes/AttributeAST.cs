using QuickCode.AST.Expressions;

namespace QuickCode.AST.Attributes;

public record class AttributeCreationAST(
    TypeIdentifierAST AttributeType,
    ListAST<ExpressionAST> Arguments,
    ListAST<(string Name, ExpressionAST Value)> NamedArguments
) : AST;
