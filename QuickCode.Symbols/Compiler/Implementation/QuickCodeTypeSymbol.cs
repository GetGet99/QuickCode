using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols.Compiler.Implementation;

public record class QuickCodeTypeSymbol(object Type, ITypeSymbol BaseType) : IUserTypeSymbol, ITypeSymbolWritable
{
    public IFieldSymbolTableWritable Fields { get; } = new QuickCodeFieldSymbolTable(BaseType.Fields);
    public IFuncSymbolTableWritable Functions { get; } = new QuickCodeFuncSymbolTable(BaseType.Functions);
    public IFuncSymbolTableWritable StaticFunctions { get; } = new QuickCodeFuncSymbolTable(BaseType.Functions);
    public IConstructorSymbolTableWritable Constructors { get; } = new QuickCodeConstructorSymbolTable();

    IFieldSymbolTable ITypeSymbol.Fields => Fields;
    IFuncSymbolTable ITypeSymbol.Functions => Functions;
    IConstructorSymbolTable ITypeSymbol.Constructors => Constructors;
    public ITypeSymbolTable Types => throw new NotImplementedException();
}
