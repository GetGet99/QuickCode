namespace QuickCode.Symbols.SymbolTables;

public interface IScopeSymbolTable
{
    IFieldSymbolTable Fields { get; }
    IVariableSymbolTableWritable Variables { get; }
    IFuncSymbolTable Functions { get; }
    //IConstructorSymbolTable Constructors { get; }
    ITypeSymbolTable Types { get; }
    bool CanDeclareVariable(string name);
    IScopeSymbolTable Clone();
}
