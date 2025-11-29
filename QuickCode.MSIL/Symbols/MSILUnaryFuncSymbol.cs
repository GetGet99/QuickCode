using Mono.Cecil;
using QuickCode.Symbols;
using QuickCode.Symbols.Functions;
using QuickCode.Symbols.Operators;

namespace QuickCode.MSIL.Symbols;

class MSILUnaryFuncSymbol(MethodDefinition method) : MSILUnaryBaseFuncSymbol(method), IUnaryOperatorFuncSymbol
{
    public UnaryOperators Operator => MSILOperatorMap.TryToUnaryOperator(method.Name, out var op) ? op : op;
}
class MSILUnaryWriteFuncSymbol(MethodDefinition method, bool before) : MSILUnaryBaseFuncSymbol(method), IUnaryWriteOperatorFuncSymbol
{
    public UnaryWriteOperators Operator => before ?
        (MSILOperatorMap.TryToUnaryWriteOpeartorBefore(method.Name, out var op) ? op : op) :
        (MSILOperatorMap.TryToUnaryWriteOpeartorAfter(method.Name, out var op2) ? op2 : op2);
}
abstract class MSILUnaryBaseFuncSymbol(MethodDefinition method) : IUnaryOperatorBaseFuncSymbol
{
    public ITypeSymbol? DefiningType => method.DeclaringType.Symbol();

    ITypeSymbol IFuncBaseSymbol.ReturnType => new MSILTypeSymbol(method.ReturnType.Resolve());

    ITypeSymbol IUnaryOperatorBaseFuncSymbol.InputType => new MSILTypeSymbol(method.Parameters[0].ParameterType.Resolve());
}
