using System.Collections.Immutable;

namespace QuickCode.Symbols;

public record class UserFuncSymbol(FunctionAST Function, ImmutableArray<(TypeSymbol Type, string Name)> Parameters, TypeSymbol ReturnType) : SingleFuncSymbol(Parameters, ReturnType);
