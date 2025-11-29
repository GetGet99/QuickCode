using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols;
public interface ITypeSymbol : ISymbol
{
    ITypeSymbol BaseType { get; }
    IFieldSymbolTable Fields { get; }
    IFuncSymbolTable Functions { get; }
    IConstructorSymbolTable Constructors { get; }
    ITypeSymbolTable Types { get; }
}
public interface ITypeSymbolWritable : ITypeSymbol
{
    new ITypeSymbol BaseType { get; }
    new IFieldSymbolTableWritable Fields { get; }
    new IFuncSymbolTableWritable Functions { get; }
    new IConstructorSymbolTableWritable Constructors { get; }
}
