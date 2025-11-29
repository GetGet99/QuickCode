namespace QuickCode.Symbols.Functions;

public record struct ParameterInfo(string Name, ITypeSymbol Type, bool HasDefaultParameter, object? DefaultParameter);
