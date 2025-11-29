using Mono.Cecil;
using Mono.Cecil.Cil;
using QuickCode.AST.Expressions;
using QuickCode.Symbols.Compiler.Implementation;
using QuickCode.Symbols.Factories;
using QuickCode.Symbols.SymbolTables;
using System.Collections.Immutable;

namespace QuickCode.Symbols;

public record class CodeGenUtils(
    ModuleDefinition Module,
    ILProcessor IL,
    Dictionary<string, VariableDefinition> Locals,
    Dictionary<string, FieldDefinition> SpilledLocals,
    Dictionary<string, Instruction> LocalGotoLabels,
    Dictionary<QuickCodeFuncSymbol, MethodDefinition> Functions,
    Instruction? ExitLabel,
    Instruction? BreakLabel,
    Instruction? ContinueLabel,
    Dictionary<string, Instruction> ExitLabels,
    Dictionary<string, Instruction> BreakLabels,
    Dictionary<string, Instruction> ContinueLabels,
    IScopeSymbolTable Symbols,
    TypeDefinition ContainingClass,
    ITypeFactory TypeFactory,
    string CurrentScopeNamespace
);