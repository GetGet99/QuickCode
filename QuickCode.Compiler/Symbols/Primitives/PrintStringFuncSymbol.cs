using Mono.Cecil.Cil;
using QuickCode.AST.Expressions;
using QuickCode.Symbols;
using QuickCode.Symbols.Factories;
using QuickCode.Symbols.Functions;

namespace QuickCode.Compiler.Symbols.Primitives;

public record class PrintStringFuncSymbol(ITypeFactory Factory) : NativeFuncSymbol
{
    public override string Name => "Print";
    public override ITypeSymbol? DefiningType => null;
    public override ParameterInfo[] Parameters => [new("value", Factory.String, false, null)];
    public override ITypeSymbol ReturnType => Factory.Void;
    public override void CodeGen(CodeGenUtils u)
    {
        u.IL.Emit(OpCodes.Call, u.Module.ImportReference(typeof(Console).GetMethod("WriteLine", [typeof(string)])!));
    }
}
