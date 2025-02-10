using QuickCode.AST.Expressions.Values;
using QuickCode.AST.Expressions;
using QuickCode.AST.Statements;
using QuickCode.Symbols;
using QuickCode.AST.TopLevels;
using System.Diagnostics.CodeAnalysis;
using QuickCode.AST;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace QuickCode.MSIL;

public class QuickCodeMSILGen
{
    /// <summary>
    /// Takes in a valid, type checked program. And fill in the IL in the method builder.
    /// This method has undefined behavior if it takes in invalid program.
    /// </summary>
    /// <returns>The main method</returns>
    public MethodDefinition CodeGen(TopLevelQuickCodeProgramAST program, ModuleDefinition module)
    {
        //var prog = module.DefineType(
        //    "<compilergenerated>.Program"
        //);
        var mainProgramClass = new TypeDefinition(
            "compilergenerated",
            "Program",
            TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.SpecialName,
            module.ImportReference(module.TypeSystem.Object)
        );
        module.Types.Add(mainProgramClass);
        var mainMethod = new MethodDefinition(
            "compilergenerated_Main",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName,
            module.ImportReference(module.TypeSystem.Void)
        );
        mainProgramClass.Methods.Add(mainMethod);
        var ilGen = mainMethod.Body.GetILProcessor();
        ArgumentNullException.ThrowIfNull(program);
        var endLabel = ilGen.Create(OpCodes.Nop);
        CodeGenUtils u = new(
            Module: module,
            IL: ilGen,
            Functions: [],
            Locals: [],
            SpilledLocals: [],
            LocalGotoLabels: [],
            ExitLabel: endLabel,
            BreakLabel: null,
            ContinueLabel: null,
            ContinueLabels: [],
            BreakLabels: [],
            ExitLabels: [], // function can't be labeled yet
            Symbols: program.Symbols,
            ContainingClass: mainProgramClass,
            CurrentScopeNamespace: "compilergenerated_Main"
        );
        GenerateFieldForCurrentLevel(u);
        foreach (var comp in program.TopLevelProgramComponentASTs)
            CodeGen(comp, u);
        u.IL.Append(endLabel);
        u.IL.Emit(OpCodes.Ret);
        //module.CreateGlobalFunctions();
        //mainProgramClass.CreateType();l
        return mainMethod;
    }
    void GenerateFieldForCurrentLevel(CodeGenUtils u)
    {
        foreach (var (name, sym) in u.Symbols.CurrentLevelSymbolKeys())
        {
            if (sym is LocalVarSymbol local && local.HasChildFunctionAccess)
            {
                var field = new FieldDefinition(
                    $"{u.CurrentScopeNamespace}.{name}",
                    FieldAttributes.Private | FieldAttributes.Static,
                    u.GetTypeRef(local.Type)
                );
                u.SpilledLocals[name] = field;
                u.ContainingClass.Fields.Add(field);
            }
        }
    }
    void CodeGen(ITopLevelDeclarable toplevel, CodeGenUtils u)
    {
        switch (toplevel)
        {
            case StatementAST stmt:
                CodeGen(stmt, u);
                break;
            case FunctionAST func:
                CodeGen(func, u);
                break;
            default:
                NotImplemented((AST.AST)toplevel);
                break;
        }
    }
    void CodeGen(ListAST<StatementAST> statements, CodeGenUtils u)
    {
        foreach (var stmt in statements)
            CodeGen(stmt, u);
    }
    void CodeGen(StatementAST statement, CodeGenUtils u)
    {
        switch (statement)
        {
            case ExprStatementAST exprStmt:
                CodeGen(exprStmt.Expression, u);
                u.IL.Emit(OpCodes.Pop);
                break;
            case LocalVarDeclStatementAST declStmt:
                CodeGen(declStmt, u);
                break;
            case IfStatementAST ifStmt:
                CodeGen(ifStmt, u);
                break;
            case WhileStatementAST whileStmt:
                CodeGen(whileStmt, u);
                break;
            case LabelStatementAST labelStmt:
                CodeGen(labelStmt, u);
                break;
            case GotoStatementAST gotoStmt:
                CodeGen(gotoStmt, u);
                break;
            case ForEachStatementAST forEachStmt:
                CodeGen(forEachStmt, u);
                break;
            case ReturnStatementAST retStmt:
                CodeGen(retStmt, u);
                break;
            case ConditionalControlFlowStatementAST controlFlowStmt:
                CodeGen(controlFlowStmt, u);
                break;
            default:
                NotImplemented(statement);
                break;
        }
    }
    void CodeGen(ExpressionAST expression, CodeGenUtils u)
    {
        switch (expression)
        {
            case UnaryAST unary:
                CodeGen(unary, u);
                break;
            case UnaryWriteAST unaryWrite:
                CodeGen(unaryWrite, u);
                break;
            case BinaryAST binary:
                CodeGen(binary, u);
                break;
            case ValueAST value:
                CodeGen(value, u);
                break;
            case IdentifierAST id:
                CodeGen(id, u);
                break;
            case AssignAST assign:
                CodeGen(assign, u);
                break;
            case FuncCallAST funcCall:
                CodeGen(funcCall, u);
                break;
            default:
                NotImplemented(expression);
                break;
        }
    }
    void CodeGen(UnaryAST unaryAST, CodeGenUtils u)
    {
        // assuming all integer operations for now
        CodeGen(unaryAST.Expression, u);
        if (unaryAST.Expression.Type.Children[unaryAST.Operator] is not SingleFuncSymbol funcSymbol)
        {
            NotImplemented(unaryAST);
            return;
        }
        if (funcSymbol is NativeFuncSymbol nativeFunc)
        {
            nativeFunc.CodeGen(null!, u);
        }
        else
        {
            NotImplemented(unaryAST);
            return;
        }
    }
    void CodeGen(UnaryWriteAST unaryWriteAST, CodeGenUtils u)
    {
        // assuming all integer operations for now
        CodeGen(unaryWriteAST.Target, u);
        switch (unaryWriteAST.Operator)
        {
            case UnaryWriteOperators.DecrementBefore:
                u.IL.Emit(OpCodes.Ldc_I4, 1);
                u.IL.Emit(OpCodes.Sub);
                u.IL.Emit(OpCodes.Dup);
                Store(unaryWriteAST.Target, u);
                break;
            case UnaryWriteOperators.IncrementBefore:
                u.IL.Emit(OpCodes.Ldc_I4, 1);
                u.IL.Emit(OpCodes.Add);
                u.IL.Emit(OpCodes.Dup);
                Store(unaryWriteAST.Target, u);
                break;
            case UnaryWriteOperators.DecrementAfter:
                u.IL.Emit(OpCodes.Dup);
                u.IL.Emit(OpCodes.Ldc_I4, 1);
                u.IL.Emit(OpCodes.Sub);
                Store(unaryWriteAST.Target, u);
                break;
            case UnaryWriteOperators.IncrementAfter:
                u.IL.Emit(OpCodes.Dup);
                u.IL.Emit(OpCodes.Ldc_I4, 1);
                u.IL.Emit(OpCodes.Add);
                Store(unaryWriteAST.Target, u);
                break;
            default:
                NotImplemented(unaryWriteAST);
                break;
        }
    }
    void CodeGen(BinaryAST binaryAST, CodeGenUtils u)
    {
        // assuming all integer operations for now
        CodeGen(binaryAST.Left, u);
        CodeGen(binaryAST.Right, u);
        if (binaryAST.Left.Type.Children[binaryAST.Operator] is not SingleFuncSymbol funcSymbol)
        {
            NotImplemented(binaryAST);
            return;
        }
        if (funcSymbol is NativeFuncSymbol nativeFunc)
        {
            nativeFunc.CodeGen(null!, u);
        } else
        {
            NotImplemented(binaryAST);
            return;
        }
    }
    void CodeGen(AssignAST assign, CodeGenUtils u)
    {
        CodeGen(assign.Right, u);
        u.IL.Emit(OpCodes.Dup); // dupe so that this expression "returns" the value of the RHS
        Store(assign.Left, u);
    }
    void CodeGen(FuncCallAST funcCall, CodeGenUtils u)
    {
        var funcSymbol = (SingleFuncSymbol)u.Symbols[funcCall.FunctionName.Name]!;
        // generate all arguments
        foreach (var arg in funcCall.Arguments)
            CodeGen(arg, u);
        if (funcSymbol is NativeFuncSymbol native)
        {
            native.CodeGen(funcCall, u);
        }
        else if (funcSymbol is UserFuncSymbol userFunc)
        {
            u.IL.Emit(OpCodes.Call, u.Functions[userFunc]);
        }
        if (funcSymbol.ReturnType == TypeSymbol.Void)
        {
            // loads in the null as a replacement of the return value
            // if return type is void
            u.IL.Emit(OpCodes.Ldnull);
        }
    }
    void CodeGen(LocalVarDeclStatementAST decl, CodeGenUtils u)
    {
        var varSymbol = u.Symbols[decl.Name.Name];
        if (varSymbol is LocalVarSymbol local && !local.HasChildFunctionAccess)
            // declare local if it is not spilled
            u.Locals[decl.Name.Name] = u.IL.DeclareLocal(u.GetTypeRef(decl.Name.Type));
        CodeGen(decl.Expression, u);
        Store(decl.Name, u);
    }
    void CodeGen(IdentifierAST id, CodeGenUtils u)
        => Load(id, u);
    void Load(IdentifierAST id, CodeGenUtils u)
    {
        var varSymbol = u.Symbols[id.Name];
        if (varSymbol is ParameterVarSymbol param)
        {
            if (param.HasChildFunctionAccess)
                NotImplemented(id);
            u.IL.Emit(OpCodes.Ldarg, param.Index);
        }
        else if (varSymbol is LocalVarSymbol local)
        {
            if (local.HasChildFunctionAccess)
                u.IL.Emit(OpCodes.Ldsfld, u.SpilledLocals[id.Name]);
            else
                u.IL.Emit(OpCodes.Ldloc, u.Locals[id.Name]);
        }
    }
    void Store(IdentifierAST id, CodeGenUtils u)
    {
        var varSymbol = u.Symbols[id.Name];
        if (varSymbol is ParameterVarSymbol param)
        {
            if (param.HasChildFunctionAccess)
                NotImplemented(id);
            u.IL.Emit(OpCodes.Starg, param.Index);
        }
        else if (varSymbol is LocalVarSymbol local)
        {
            if (local.HasChildFunctionAccess)
                u.IL.Emit(OpCodes.Stsfld, u.SpilledLocals[id.Name]);
            else
                u.IL.Emit(OpCodes.Stloc, u.Locals[id.Name]);
        }
    }
    void CodeGen(IfStatementAST ifStmt, CodeGenUtils u)
    {
        var startIfLabel = u.IL.DefineLabel();
        var falseLabel = u.IL.DefineLabel();
        var endIfLabel = u.IL.DefineLabel();
        var uInner = u with
        {
            ExitLabel = endIfLabel,
            LocalGotoLabels = CloneDict(u.LocalGotoLabels),
            ExitLabels = CloneDict(u.ExitLabels)
        };
        if (ifStmt.Label is { } lb)
        {
            u.LocalGotoLabels[lb.Name] = startIfLabel;
            uInner.ExitLabels[lb.Name] = endIfLabel;
        }
        u.IL.MarkLabel(startIfLabel);
        CodeGen(ifStmt.Condition, u);
        // if false, skip
        u.IL.Emit(OpCodes.Brfalse, falseLabel);
        // true
        CodeGenBlock(ifStmt.TrueBlock, uInner);
        u.IL.Emit(OpCodes.Br, endIfLabel);
        // false
        u.IL.MarkLabel(falseLabel);
        CodeGenBlock(ifStmt.FalseBlock, uInner);
        // end if
        u.IL.MarkLabel(endIfLabel);
    }
    void CodeGen(WhileStatementAST whileDecl, CodeGenUtils u)
    {
        var loopEnterLabel = u.IL.DefineLabel();
        var loopExitLabel = u.IL.DefineLabel();
        var continueLabel = whileDecl.IsDoWhileStmt ? u.IL.DefineLabel() : loopEnterLabel;
        var uInner = u with
        {
            ExitLabel = loopExitLabel,
            BreakLabel = loopExitLabel,
            ContinueLabel = continueLabel,
            LocalGotoLabels = CloneDict(u.LocalGotoLabels),
            ExitLabels = CloneDict(u.ExitLabels),
            ContinueLabels = CloneDict(u.ContinueLabels),
            BreakLabels = CloneDict(u.BreakLabels),
        };
        if (whileDecl.Label is { } lb)
        {
            u.LocalGotoLabels[lb.Name] = loopEnterLabel;
            uInner.LocalGotoLabels[lb.Name] = loopEnterLabel;
            uInner.ExitLabels[lb.Name] = loopExitLabel;
            uInner.BreakLabels[lb.Name] = loopExitLabel;
            uInner.ContinueLabels[lb.Name] = continueLabel;
        }
        u.IL.MarkLabel(loopEnterLabel);
        if (!whileDecl.IsDoWhileStmt)
        {
            // check the loop condition first
            CodeGen(whileDecl.Condition, u);
            // if false, exit
            u.IL.Emit(OpCodes.Brfalse, loopExitLabel);
        }
        CodeGenBlock(whileDecl.Block, uInner);

        if (!whileDecl.IsDoWhileStmt)
        {
            // go back to beginning
            u.IL.Emit(OpCodes.Br, loopEnterLabel);
        }
        else
        {
            // do-while continue checks condition first
            u.IL.MarkLabel(continueLabel);
            // check the loop condition
            CodeGen(whileDecl.Condition, u);
            // go back if true
            u.IL.Emit(OpCodes.Brtrue, loopEnterLabel);
        }
        u.IL.MarkLabel(loopExitLabel);
    }
    void CodeGen(ForEachStatementAST forEachStmt, CodeGenUtils u)
    {
        // NOTE: goto labels on for each statement ALWAYS re-evaluate the list argument
        if (forEachStmt.List is not BinaryAST range || range.Operator is not BinaryOperators.Range)
        {
            // non range iteration not yet implemented
            NotImplemented(forEachStmt.List);
            return;
        }
        var gotoLoopEnterLabel = u.IL.DefineLabel();
        var u2 = CreateLocalScope(forEachStmt.Block, u);
        VariableDefinition userIterVar;
        if (forEachStmt.UseOutterTarget)
        {
            userIterVar = u.Locals[forEachStmt.Target.Name];
        }
        else
        {
            userIterVar = u2.Locals[forEachStmt.Target.Name] = u2.IL.DeclareLocal(u.GetTypeRef(TypeSymbol.Int32));
        }
        var i = u.IL.DeclareLocal(u.GetTypeRef(TypeSymbol.Int32));
        var startVar = u.IL.DeclareLocal(u.GetTypeRef(TypeSymbol.Int32));
        var endVar = u.IL.DeclareLocal(u.GetTypeRef(TypeSymbol.Int32));
        var innerLoopLabel = u.IL.DefineLabel();
        var loopExitLabel = u.IL.DefineLabel();
        var continueLabel = u.IL.DefineLabel();

        u.IL.MarkLabel(gotoLoopEnterLabel);

        u2 = u2 with {
            BreakLabel = loopExitLabel,
            ExitLabel = loopExitLabel,
            ContinueLabel = continueLabel,
            ExitLabels = CloneDict(u2.ExitLabels),
            ContinueLabels = CloneDict(u2.ContinueLabels),
            BreakLabels = CloneDict(u2.BreakLabels),
        };
        if (forEachStmt.Label is { } lb)
        {
            u.LocalGotoLabels[lb.Name] = gotoLoopEnterLabel;
            u2.ExitLabels[lb.Name] = loopExitLabel;
            u2.BreakLabels[lb.Name] = loopExitLabel;
            u2.ContinueLabels[lb.Name] = continueLabel;
        }
        // generate the start range
        CodeGen(range.Left, u);
        u.IL.Emit(OpCodes.Stloc, startVar);
        // generate the end range
        CodeGen(range.Right, u);
        // endVar = end
        u.IL.Emit(OpCodes.Stloc, endVar);


        // i = start
        u.IL.Emit(OpCodes.Ldloc, startVar);
        u.IL.Emit(OpCodes.Stloc, i);


        u.IL.MarkLabel(innerLoopLabel);
        // if i == end, exit
        u.IL.Emit(OpCodes.Ldloc, i);
        u.IL.Emit(OpCodes.Ldloc, endVar);
        u.IL.Emit(OpCodes.Beq, loopExitLabel);

        // assign the variable for the user
        u.IL.Emit(OpCodes.Ldloc, i);
        u.IL.Emit(OpCodes.Stloc, userIterVar);

        // execute user code with u2
        CodeGen(forEachStmt.Block.Statements, u2);

        // next iteration prep
        u.IL.MarkLabel(continueLabel);
        // increment i
        u.IL.Emit(OpCodes.Ldloc, i);
        u.IL.Emit(OpCodes.Ldc_I4, 1);
        u.IL.Emit(OpCodes.Add);
        u.IL.Emit(OpCodes.Stloc, i);
        // also bring changes to the user iter var
        u.IL.Emit(OpCodes.Ldloc, i);
        u.IL.Emit(OpCodes.Stloc, userIterVar);

        // go back to beginning
        u.IL.Emit(OpCodes.Br, innerLoopLabel);

        u.IL.MarkLabel(loopExitLabel);
    }
    void CodeGen(LabelStatementAST labelStmt, CodeGenUtils u)
    {
        if (u.LocalGotoLabels.TryGetValue(labelStmt.Label.Name, out var label))
        {
            // the forward goto is not yet implemented
            NotImplemented(labelStmt);
        }
        u.LocalGotoLabels[labelStmt.Label.Name] = label = u.IL.DefineLabel();
        u.IL.MarkLabel(label);
    }
    void CodeGen(ReturnStatementAST returnStmt, CodeGenUtils u)
    {
        Instruction? skipReturnLabel = null;
        if (returnStmt.Condition is { } condExpr)
        {
            skipReturnLabel = u.IL.DefineLabel();
            CodeGen(condExpr, u);
            u.IL.Emit(OpCodes.Brfalse, skipReturnLabel);
        }
        if (returnStmt.ReturnValue is { } retExpr)
        {
            CodeGen(retExpr, u);
        }
        u.IL.Emit(OpCodes.Ret);
        if (skipReturnLabel is { } lb)
        {
            u.IL.MarkLabel(lb);
        }
    }
    void CodeGen(ConditionalControlFlowStatementAST controlFlowStmt, CodeGenUtils u)
    {
        var label = controlFlowStmt switch
        {
            BreakStatementAST b => b.Label is { } lb ? u.BreakLabels[lb.Name] : u.BreakLabel,
            ContinueStatementAST c => c.Label is { } lb ? u.ContinueLabels[lb.Name] : u.ContinueLabel,
            ExitStatementAST e => e.Label is { } lb ? u.ExitLabels[lb.Name] : u.ExitLabel,
            _ => NotImplemented<Instruction?>(controlFlowStmt)
        };
        if (controlFlowStmt.Condition is { } condExpr)
        {
            CodeGen(condExpr, u);
            u.IL.Emit(OpCodes.Brtrue, label);
        } else
        {
            u.IL.Emit(OpCodes.Br, label);
        }
    }
    void CodeGen(GotoStatementAST gotoStmt, CodeGenUtils u)
    {
        if (!u.LocalGotoLabels.TryGetValue(gotoStmt.Label.Name, out var label))
        {
            // the forward goto is not yet implemented
            NotImplemented(gotoStmt);
        }
        if (gotoStmt.Condition is { } condition)
        {
            CodeGen(condition, u);
            u.IL.Emit(OpCodes.Brtrue, label);
        }
        else
        {
            u.IL.Emit(OpCodes.Br, label);
        }
    }
    void CodeGen(FunctionAST funcAST, CodeGenUtils u)
    {
        var funcSymbol = (UserFuncSymbol)u.Symbols[funcAST.Name.Name]!;
        var method = new MethodDefinition(
            funcAST.Name.Name,
            MethodAttributes.Private | MethodAttributes.Static,
            u.GetTypeRef(funcSymbol.ReturnType)
        );
        u.ContainingClass.Methods.Add(method);
        foreach (var ts in funcSymbol.Parameters)
            method.Parameters.Add(new ParameterDefinition(ts.Name, ParameterAttributes.None, u.GetTypeRef(ts.Type)));
        u.Functions[funcSymbol] = method;
        var childIlGen = method.Body.GetILProcessor();
        var ExitLabel = funcSymbol.ReturnType == TypeSymbol.Void ? childIlGen.DefineLabel() : null;
        CodeGenUtils uChild = new(
            Module: u.Module,
            IL: method.Body.GetILProcessor(),
            Functions: CloneDict(u.Functions),
            Locals: [],
            SpilledLocals: CloneDict(u.SpilledLocals),
            LocalGotoLabels: [],
            BreakLabel: null,
            ContinueLabel: null,
            ExitLabel: ExitLabel,
            ContinueLabels: [],
            BreakLabels: [],
            ExitLabels: [], // function can't be labeled yet
            Symbols: funcAST.SymbolTable,
            ContainingClass: u.ContainingClass,
            CurrentScopeNamespace: $"{u.CurrentScopeNamespace}.{funcAST.Name}"
        );
        for (int i = 0; i < funcSymbol.Parameters.Length; i++)
        {
            var paramSym = (ParameterVarSymbol)uChild.Symbols[funcSymbol.Parameters[i].Name]!;
            if (paramSym.HasChildFunctionAccess)
            {
                NotImplemented(funcAST);
            }
        }
        CodeGen(funcAST.Statements, uChild);
        if (funcSymbol.ReturnType == TypeSymbol.Void)
        {
            uChild.IL.Append(ExitLabel);
            uChild.IL.Emit(OpCodes.Ret);
        }
    }
    void CodeGenBlock(BlockControlAST block, CodeGenUtils outterScopeUtils)
    {
        if (block.Statements.Count is 0)
            // we can fast path this
            return;

        CodeGen(block.Statements, CreateLocalScope(block, outterScopeUtils));
    }
    CodeGenUtils CreateLocalScope(BlockControlAST block, CodeGenUtils u)
    {
        // we don't want inner scope to change the local for the outter scope
        // and also set inner symbols
        var u2 = u with { Locals = CloneDict(u.Locals), Symbols = block.InnerSymbols };
        return u2;
    }
    void CodeGen(ValueAST value, CodeGenUtils u)
    {
        switch (value)
        {
            case Int32ValueAST intValue:
                u.IL.Emit(OpCodes.Ldc_I4, intValue.Value);
                break;
            case BooleanValueAST boolValue:
                u.IL.Emit(OpCodes.Ldc_I4, boolValue.Value ? 1 : 0);
                break;
            case StringValueAST stringValue:
                u.IL.Emit(OpCodes.Ldstr, stringValue.Value);
                break;
            case ArrayDeclarationWithValuesAST arr:
                u.IL.Emit(OpCodes.Ldc_I4, arr.Elements.Count); // count
                u.IL.Emit(OpCodes.Newarr, u.GetTypeRef(arr.Type)); // arr
                for (int i = 0; i < arr.Elements.Count; i++)
                {
                    u.IL.Emit(OpCodes.Dup); // arr, arr
                    u.IL.Emit(OpCodes.Ldc_I4, i); // arr, arr, i
                    CodeGen(arr.Elements[i], u); // arr, arr, i, ele[i]
                    u.IL.Emit(OpCodes.Stelem_I4); // arr
                }
                break;
            default:
                NotImplemented(value);
                break;
        }
    }
    Dictionary<K, V> CloneDict<K, V>(Dictionary<K, V> dict)
        where K : notnull
    {
        var d2 = new Dictionary<K, V>();
        foreach (var (k, v) in dict)
        {
            d2[k] = v;
        }
        return d2;
    }
    [DoesNotReturn]
    static void NotImplemented(AST.AST node)
    {
        throw new NotImplementedException($"The node of type {node.GetType().Name} is not implemented.");
    }
    [DoesNotReturn]
    static T NotImplemented<T>(AST.AST node)
    {
        NotImplemented(node);
        return default;
    }
}
public static class Extension
{
    public static Instruction DefineLabel(this ILProcessor il)
    {
        return il.Create(OpCodes.Nop);
    }
    public static void MarkLabel(this ILProcessor il, Instruction label)
    {
        il.Append(label);
    }
    public static VariableDefinition DeclareLocal(this ILProcessor il, TypeReference typeRef)
    {
        var local = new VariableDefinition(typeRef);
        il.Body.Variables.Add(local);
        return local;
    }
    public static TypeReference GetTypeRef(this CodeGenUtils u, TypeSymbol type)
    {
        if (type == TypeSymbol.Int32)
        {
            return u.Module.TypeSystem.Int32;
        }
        else if (type == TypeSymbol.Void)
        {
            return u.Module.TypeSystem.Void;
        }
        else
        {
            return u.Module.TypeSystem.Object;
        }
    }
}