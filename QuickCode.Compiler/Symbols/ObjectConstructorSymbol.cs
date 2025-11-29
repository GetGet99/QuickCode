using Mono.Cecil.Cil;
using QuickCode.AST.Expressions;
using QuickCode.Symbols;
using QuickCode.Symbols.Factories;
using QuickCode.Symbols.Functions;

namespace QuickCode.Compiler.Symbols;

public record class ObjectConstructorSymbol(ITypeFactory Factory) : NativeConstructorSymbol
{
    public override ITypeSymbol? DefiningType => Factory.Object;
    public override ParameterInfo[] Parameters => [];
    public override ITypeSymbol ReturnType => Factory.Object;

    public override void CodeGenCall(FuncCallAST funcCall, CodeGenUtils u)
    {
        u.IL.Emit(OpCodes.Call, u.Module.ImportReference(typeof(object).GetConstructor([])!));
    }
    public override void CodeGenNew(FuncCallAST funcCall, CodeGenUtils u)
    {
        u.IL.Emit(OpCodes.Newobj, u.Module.ImportReference(typeof(object).GetConstructor([])!));
    }
}
