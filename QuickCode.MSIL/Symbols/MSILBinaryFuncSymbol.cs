using Mono.Cecil;
using QuickCode.Symbols;
using QuickCode.Symbols.Functions;
using QuickCode.Symbols.Operators;

namespace QuickCode.MSIL.Symbols;

class MSILBinaryFuncSymbol(MethodDefinition method) : IBinaryOperatorFuncSymbol
{
    public ITypeSymbol? DefiningType => method.DeclaringType.Symbol();

    public BinaryOperators Operator => MSILOperatorMap.TryToBianryOperator(method.Name, out var op) ? op : op;

    ITypeSymbol IFuncBaseSymbol.ReturnType => new MSILTypeSymbol(method.ReturnType.Resolve());

    ITypeSymbol IBinaryOperatorFuncSymbol.LeftInputType => new MSILTypeSymbol(method.Parameters[0].ParameterType.Resolve());
    ITypeSymbol IBinaryOperatorFuncSymbol.RightInputType => new MSILTypeSymbol(method.Parameters[1].ParameterType.Resolve());
}