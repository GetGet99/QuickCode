using QuickCode.Symbols.Functions;

namespace QuickCode.Symbols.SymbolTables;

public interface IConstructorSymbolTable
{
    IConstructorSymbol? this[ArgumentInfo[] args] { get; }
    IEnumerable<IConstructorSymbol> Constructors { get; }
}
public interface IConstructorSymbolTableWritable : IConstructorSymbolTable
{
    new IConstructorSymbol? this[ArgumentInfo[] args] { set; }
}