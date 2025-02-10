namespace QuickCode.Symbols;

public record class ParameterVarSymbol(int Index, TypeSymbol Type) : LocalVarSymbol(Type);
