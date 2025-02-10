using System.Collections.Immutable;

namespace QuickCode.Symbols;

public record class UserFuncSymbol(object Function, ImmutableArray<(TypeSymbol Type, string Name)> Parameters, TypeSymbol ReturnType) : SingleFuncSymbol(Parameters, ReturnType);
