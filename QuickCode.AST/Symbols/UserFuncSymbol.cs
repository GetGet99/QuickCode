using System.Collections.Immutable;

namespace QuickCode.AST.Symbols;

public record class UserFuncSymbol(FunctionAST Function, ImmutableArray<(TypeSymbol Type, string Name)> Parameters, TypeSymbol ReturnType) : FuncSymbol(Parameters, ReturnType);
