using Mono.Cecil;
using Mono.Cecil.Cil;
using QuickCode.AST.Expressions;
using System.Collections.Immutable;

namespace QuickCode.AST.Symbols;

public record class CodeGenUtils(
    ModuleDefinition Module,
    ILProcessor IL,
    Dictionary<string, VariableDefinition> Locals,
    Dictionary<string, FieldDefinition> SpilledLocals,
    Dictionary<string, Instruction> LocalGotoLabels,
    Dictionary<UserFuncSymbol, MethodDefinition> Functions,
    Instruction? ExitLabel,
    Instruction? BreakLabel,
    Instruction? ContinueLabel,
    Dictionary<string, Instruction> ExitLabels,
    Dictionary<string, Instruction> BreakLabels,
    Dictionary<string, Instruction> ContinueLabels,
    SymbolTable Symbols,
    TypeDefinition ContainingClass,
    string CurrentScopeNamespace
);
public abstract record class NativeFuncSymbol(ImmutableArray<(TypeSymbol Type, string Name)> Parameters, TypeSymbol ReturnType) : FuncSymbol(Parameters, ReturnType)
{
    /// <summary>
    /// Generaets the code for the given function call.
    /// The arguments are already provided on the stack.
    /// </summary>
    /// <param name="funcCall">The function call AST</param>
    /// <param name="u">The code gen utils</param>
    public abstract void CodeGen(FuncCallAST funcCall, CodeGenUtils u);
}
public record class PrintFuncSymbol : NativeFuncSymbol
{
    public static PrintFuncSymbol Singleton { get; } = new();
    private PrintFuncSymbol() : base(
        [(TypeSymbol.Object, "value")],
        TypeSymbol.Void
    )
    {

    }
    public override void CodeGen(FuncCallAST funcCall, CodeGenUtils u)
    {
        var type = funcCall.Arguments[0].Type;
        if (type == TypeSymbol.Int32)
        {
            u.IL.Emit(OpCodes.Call, u.Module.ImportReference(typeof(Console).GetMethod("WriteLine", [typeof(int)])!));
        }
        else if (type == TypeSymbol.Boolean)
        {
            u.IL.Emit(OpCodes.Call, u.Module.ImportReference(typeof(Console).GetMethod("WriteLine", [typeof(bool)])!));
        }
        else
        {
            u.IL.Emit(OpCodes.Call, u.Module.ImportReference(typeof(Console).GetMethod("WriteLine", [typeof(object)])!));
        }
    }
}