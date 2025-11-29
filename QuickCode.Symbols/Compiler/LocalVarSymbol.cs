namespace QuickCode.Symbols.Compiler;

public record class LocalVarSymbol(ITypeSymbol Type) : VarSymbol(Type)
{
    public bool HasChildFunctionAccess { get; internal set; } = false;
}
