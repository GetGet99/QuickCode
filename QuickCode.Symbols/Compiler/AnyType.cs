using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols.Compiler;

class AnyType : ITypeSymbol
{
    public static AnyType Instance { get; } = new AnyType();
    private AnyType() { }
    public ITypeSymbol BaseType => throw new InvalidOperationException();
    public IFieldSymbolTable Fields => throw new InvalidOperationException();
    public IFuncSymbolTable Functions => throw new InvalidOperationException();
    public IConstructorSymbolTable Constructors => throw new InvalidOperationException();
    public ITypeSymbolTable Types => throw new InvalidOperationException();
}