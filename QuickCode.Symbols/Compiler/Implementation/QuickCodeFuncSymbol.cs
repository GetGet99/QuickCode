using QuickCode.Symbols.Functions;

namespace QuickCode.Symbols.Compiler.Implementation;

public record class QuickCodeFuncSymbol(
    object Function, string Name, ITypeSymbol? DefiningType,
    ParameterInfo[] Parameters, ITypeSymbol ReturnType
) : QuickCodeVaryFuncSymbol(Function, DefiningType, Parameters, ReturnType),
    IFuncSymbol;
public abstract record class QuickCodeConstructorFuncSymbol(
    object Function, ITypeSymbol? DefiningType,
    ParameterInfo[] Parameters, ITypeSymbol ReturnType
) : QuickCodeVaryFuncSymbol(Function, DefiningType, Parameters, ReturnType),
    IConstructorSymbol;
public abstract record class QuickCodeVaryFuncSymbol(
    object Function, ITypeSymbol? DefiningType,
    ParameterInfo[] Parameters, ITypeSymbol ReturnType
) : QuickCodeFuncBaseSymbol(Function, DefiningType, ReturnType),
    IVaryParametersFuncBaseSymbol;
public abstract record class QuickCodeFuncBaseSymbol(
    object Function, ITypeSymbol? DefiningType, ITypeSymbol ReturnType
) : IFuncBaseSymbol;
