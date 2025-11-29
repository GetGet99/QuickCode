namespace QuickCode.Symbols.SymbolTables;

public interface ITypeSymbolTable
{
    ITypeSymbol? this[string name, ITypeSymbol[] parameters] { get; }
    ITypeSymbol? GetInCurrentLevel(string name, ITypeSymbol[] args);
}
public interface ITypeSymbolTableWriteable : ITypeSymbolTable
{
    new ITypeSymbol this[string name] { set; }
}