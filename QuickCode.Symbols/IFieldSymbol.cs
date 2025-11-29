namespace QuickCode.Symbols;

public interface IFieldSymbol : ISymbol
{
    ITypeSymbol Type { get; }
    string Name { get; }
}