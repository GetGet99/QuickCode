namespace QuickCode.AST.Expressions;

public record class IdentifierAST(string Name) : ExpressionAST
{
    public override string ToString()
    {
        return $"Identifier[{Name}]";
    }
}
