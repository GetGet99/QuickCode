using System.Collections.Immutable;

namespace QuickCode.Symbols;

public abstract record class SingleFuncSymbol(ImmutableArray<(TypeSymbol Type, string Name)> Parameters, TypeSymbol ReturnType) : Symbol;
