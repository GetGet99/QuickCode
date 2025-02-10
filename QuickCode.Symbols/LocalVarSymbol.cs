namespace QuickCode.Symbols;

public record class LocalVarSymbol(TypeSymbol Type) : VarSymbol(Type)
{
    public bool HasChildFunctionAccess { get; internal set; } = false;
}
