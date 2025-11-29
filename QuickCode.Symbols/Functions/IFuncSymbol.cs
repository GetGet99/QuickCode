using QuickCode.Symbols.Operators;

namespace QuickCode.Symbols.Functions;

public interface IFuncSymbol : IVaryParametersFuncBaseSymbol
{
    string Name { get; }
}
public interface IConstructorSymbol : IVaryParametersFuncBaseSymbol;
public interface IVaryParametersFuncBaseSymbol : ISymbol, IFuncBaseSymbol
{
    ParameterInfo[] Parameters { get; }
}
public interface IUnaryOperatorFuncSymbol : IUnaryOperatorBaseFuncSymbol
{
    UnaryOperators Operator { get; }
}
public interface IUnaryWriteOperatorFuncSymbol : IUnaryOperatorBaseFuncSymbol
{
    UnaryWriteOperators Operator { get; }
}
public interface IUnaryOperatorBaseFuncSymbol : ISymbol, IFuncBaseSymbol
{
    ITypeSymbol InputType { get; }
}
public interface IBinaryOperatorFuncSymbol : ISymbol, IFuncBaseSymbol
{
    BinaryOperators Operator { get; }
    ITypeSymbol LeftInputType { get; }
    ITypeSymbol RightInputType { get; }
}
public interface IFuncBaseSymbol
{
    ITypeSymbol? DefiningType { get; }
    ITypeSymbol ReturnType { get; }
}