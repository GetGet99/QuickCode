using Mono.Cecil;
using QuickCode.Symbols;
using QuickCode.Symbols.Functions;

namespace QuickCode.MSIL.Symbols;

class MSILFuncSymbol(MethodDefinition method) : IFuncSymbol
{
    public ITypeSymbol? DefiningType => method.DeclaringType.Symbol();

    ParameterInfo[] IVaryParametersFuncBaseSymbol.Parameters => [
        .. from param in method.Parameters select new ParameterInfo(param.Name, new MSILTypeSymbol(param.ParameterType.Resolve()), param.HasDefault, null)
        ];
    ITypeSymbol IFuncBaseSymbol.ReturnType => new MSILTypeSymbol(method.ReturnType.Resolve());
    string IFuncSymbol.Name => method.Name;
}

class MSILConstructorSymbol(MethodDefinition method) : IConstructorSymbol
{
    public ITypeSymbol? DefiningType => method.DeclaringType.Symbol();
    ParameterInfo[] IVaryParametersFuncBaseSymbol.Parameters => [
        .. from param in method.Parameters select new ParameterInfo(param.Name, new MSILTypeSymbol(param.ParameterType.Resolve()), param.HasDefault, null)
        ];
    ITypeSymbol IFuncBaseSymbol.ReturnType => new MSILTypeSymbol(method.ReturnType.Resolve());
}
