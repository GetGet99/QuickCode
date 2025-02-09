using System.Collections.Immutable;

namespace QuickCode.AST.Symbols;

public abstract record class FuncSymbol(ImmutableArray<(TypeSymbol Type, string Name)> Parameters, TypeSymbol ReturnType) : Symbol;
