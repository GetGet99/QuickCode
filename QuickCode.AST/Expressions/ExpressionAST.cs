using QuickCode.Symbols;

namespace QuickCode.AST.Expressions;

public abstract record class ExpressionAST : AST
{
    ITypeSymbol? _type;
    /// <summary>
    /// Represents the type of the current node.
    /// This is only valid after the expression has been type checked.
    /// </summary>
    public ITypeSymbol Type
    {
        get => _type!;
        internal set => _type = value;
    }
}
