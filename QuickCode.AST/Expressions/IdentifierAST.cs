namespace QuickCode.AST.Expressions;

public record class IdentifierAST(string Name) : ExpressionAST, IOverloadable
{
    public bool IsType { get; set; } = false;
    public override string ToString()
    {
        return $"Identifier[{Name}]";
    }
}
