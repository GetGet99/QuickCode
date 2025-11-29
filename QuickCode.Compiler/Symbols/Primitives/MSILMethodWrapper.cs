using Mono.Cecil;
using Mono.Cecil.Cil;
using QuickCode.MSIL;
using QuickCode.Symbols;
using QuickCode.Symbols.Functions;

namespace QuickCode.Compiler.Symbols.Primitives;

class MSILMethodWrapper(MSILTypeFactory factory, MethodReference methodReference) : INativeMSILImpl, IFuncSymbol
{
    public string Name => methodReference.Name;

    public ParameterInfo[] Parameters => field ??= [.. methodReference.Parameters.Select(
        x => new ParameterInfo(x.Name, factory.ResolveType(x.ParameterType), x.HasDefault, x.Constant)
    )];

    public ITypeSymbol? DefiningType => field ??= factory.ResolveType(methodReference.DeclaringType);

    public ITypeSymbol ReturnType => field ??= factory.ResolveType(methodReference.ReturnType);

    public void CodeGen(CodeGenUtils u)
    {
        u.IL.Emit(OpCodes.Call, u.Module.ImportReference(methodReference));
    }
}