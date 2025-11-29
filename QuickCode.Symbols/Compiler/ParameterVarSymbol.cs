namespace QuickCode.Symbols.Compiler;

public record class ParameterVarSymbol(int Index, ITypeSymbol Type) : LocalVarSymbol(Type);
