namespace QuickCode.Symbols.SymbolTables;

public interface IFieldSymbolTable
{
    IFieldSymbol? this[string name] { get; }
    IEnumerable<IFieldSymbol> CurrentLevel { get; }
    IFieldSymbol? GetInCurrentLevel(string name);
}
public interface IFieldSymbolTableWritable : IFieldSymbolTable
{
    new IFieldSymbol? this[string name] { set; }
}
