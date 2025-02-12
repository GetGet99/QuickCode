using QuickCode.AST;
using QuickCode.AST.Expressions;
using QuickCode.AST.Expressions.Values;
using QuickCode.AST.Statements;
using QuickCode.Symbols;
using QuickCode.AST.TopLevels;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Immutable;

namespace QuickCode.TypeChecking;
using static TypeCheckHelper;
public class QuickCodeTypeChecker
{
    public void TypeCheck(TopLevelQuickCodeProgramAST program, SymbolTable defaultSymbolTable)
    {
        var SymbolTable = defaultSymbolTable.Clone();
        var tc = new TypeCheckState(
            SymbolTable,
            DefinedGotoLabels: [],
            HasReturned: new(),
            CanExit: true,
            CanBreak: false,
            CanContinue: false,
            ExitableLabels: [],
            BreakableLabels: [],
            ContinuableLabels: []);
        ArgumentNullException.ThrowIfNull(program);
        foreach (var comp in program.TopLevelProgramComponentASTs)
            TypeCheck(comp, tc);
        program.Symbols = SymbolTable;
    }
    void TypeCheck(ITopLevelDeclarable toplevel, TypeCheckState tc)
    {
        switch (toplevel)
        {
            case StatementAST statement:
                TypeCheck(statement, tc);
                break;
            case FunctionAST func:
                TypeCheck(func, tc);
                break;
            default:
                tc.NotImplemented((AST.AST)toplevel);
                break;
        }
    }
    void TypeCheck(ListAST<StatementAST> statements, TypeCheckState tc)
    {
        foreach (var comp in statements)
            TypeCheck(comp, tc);
    }
    void TypeCheck(StatementAST statement, TypeCheckState tc)
    {
        switch (statement)
        {
            case ExprStatementAST exprStmt:
                TypeCheck(exprStmt.Expression, tc);
                break;
            case LocalVarDeclStatementAST declStmt:
                TypeCheck(declStmt, tc);
                break;
            case IfStatementAST ifStmt:
                TypeCheck(ifStmt, tc);
                break;
            case WhileStatementAST whileStmt:
                TypeCheck(whileStmt, tc);
                break;
            case LabelStatementAST labelStmt:
                TypeCheck(labelStmt, tc);
                break;
            case GotoStatementAST gotoStmt:
                TypeCheck(gotoStmt, tc);
                break;
            case ForEachStatementAST forEachStmt:
                TypeCheck(forEachStmt, tc);
                break;
            case ConditionalControlFlowStatementAST conditionalControlFlowStatementAST:
                TypeCheck(conditionalControlFlowStatementAST, tc);
                break;
            default:
                tc.NotImplemented(statement);
                break;
        }
    }
    void TypeCheck(ExpressionAST expression, TypeCheckState tc)
    {
        switch (expression)
        {
            case UnaryAST unary:
                TypeCheck(unary, tc);
                break;
            case UnaryWriteAST unaryWrite:
                TypeCheck(unaryWrite, tc);
                break;
            case BinaryAST binary:
                TypeCheck(binary, tc);
                break;
            case ValueAST value:
                TypeCheck(value, tc);
                break;
            case IdentifierAST id:
                TypeCheck(id, tc);
                break;
            case AssignAST assign:
                TypeCheck(assign, tc);
                break;
            case FuncCallAST funcCall:
                TypeCheck(funcCall, tc);
                break;
            default:
                tc.NotImplemented(expression);
                break;
        }
    }
    void TypeCheck(UnaryAST unaryAST, TypeCheckState tc)
    {
        TypeCheck(unaryAST.Expression, tc);
        if (unaryAST.Expression.Type.Children[unaryAST.Operator] is not SingleFuncSymbol handler)
        {
            tc.NotImplemented(unaryAST);
            return;
        }
        if (tc.AssertTypeAssignableTo(unaryAST, handler.Parameters[0].Type, unaryAST.Expression.Type))
        {
            unaryAST.Type = handler.ReturnType;
        }
        else
        {
            unaryAST.Type = TypeSymbol.Object;
        }
    }
    void TypeCheck(UnaryWriteAST unaryWriteAST, TypeCheckState tc)
    {
        // manual mode for now, operator overloading still not implemented
        // let's assume only integer for now
        TypeCheck(unaryWriteAST.Target, tc);
        if (tc.AssertTypeAssignableTo(unaryWriteAST.Target, TypeSymbol.Int32, unaryWriteAST.Target.Type))
        {
            switch (unaryWriteAST.Operator)
            {
                case UnaryWriteOperators.IncrementBefore or
                    UnaryWriteOperators.IncrementAfter or
                    UnaryWriteOperators.DecrementBefore or
                    UnaryWriteOperators.DecrementAfter:
                    unaryWriteAST.Type = TypeSymbol.Int32;
                    break;
                default:
                    tc.NotImplemented(unaryWriteAST);
                    break;
            }
        }
        else
        {
            unaryWriteAST.Type = TypeSymbol.Object;
        }
    }
    void TypeCheck(BinaryAST binaryAST, TypeCheckState tc)
    {
        TypeCheck(binaryAST.Left, tc);
        TypeCheck(binaryAST.Right, tc);
        if (binaryAST.Left.Type.Children[binaryAST.Operator] is not SingleFuncSymbol handler)
        {
            tc.NotImplemented(binaryAST);
            return;
        }
        if (Combine(
            tc.AssertTypeAssignableTo(binaryAST, handler.Parameters[0].Type, binaryAST.Left.Type),
            tc.AssertTypeAssignableTo(binaryAST, handler.Parameters[1].Type, binaryAST.Right.Type)
        ))
        {
            binaryAST.Type = handler.ReturnType;
        }
        else
        {
            binaryAST.Type = TypeSymbol.Object;
        }
    }
    void TypeCheck(AssignAST assign, TypeCheckState tc, bool hasRightTypeChecked = false)
    {
        TypeCheck(assign.Left, tc);
        if (!hasRightTypeChecked)
            TypeCheck(assign.Right, tc);
        tc.AssertTypeAssignableTo(assign.Right, assign.Left.Type, assign.Right.Type);
        assign.Type = assign.Left.Type;
    }
    void TypeCheck(FuncCallAST funcCall, TypeCheckState tc)
    {
        foreach (var argument in funcCall.Arguments)
        {
            TypeCheck(argument, tc);
        }

        funcCall.Type = TypeSymbol.Void;
        var symbol = tc.SymbolTable[funcCall.FunctionName.Name];
        SingleFuncSymbol? funcSymbol;
        if (symbol is null)
        {
            tc.SymbolUndefinedError(funcCall.FunctionName);
            funcSymbol = null;
        }
        else if (symbol is not SingleFuncSymbol func)
        {
            if (symbol is OverloadedFuncSymbol overloadedFunc)
            {
                funcSymbol = overloadedFunc.Get((from v in funcCall.Arguments select v.Type).ToImmutableArray());
                if (funcSymbol is null)
                {
                    tc.NoProperOverloadedError(funcCall.FunctionName);
                }
            } else
            {
                tc.SymbolIsNotFunctionError(funcCall.FunctionName);
                funcSymbol = null;
            }
        }
        else
        {
            funcSymbol = func;
        }
        int i = 0;
        if (funcSymbol is not null)
        {
            funcCall.ResolvedOverload = funcSymbol;
            if (funcCall.Arguments.Count != funcSymbol.Parameters.Length)
            {
                tc.TypeCheckError(funcCall,
                    $"The function does accepts {funcSymbol.Parameters.Length} parameters" +
                    $", but {funcCall.Arguments.Count} arguments were given");
            }
            for (; i < funcCall.Arguments.Count && i < funcSymbol.Parameters.Length; i++)
            {
                var arg = funcCall.Arguments[i];
                tc.AssertTypeAssignableTo(arg, funcSymbol.Parameters[i].Type, arg.Type);
            }
        }
        // continue for the rest
        for (; i < funcCall.Arguments.Count; i++)
        {
            var arg = funcCall.Arguments[i];
            TypeCheck(arg, tc);
        }
        funcCall.Type = funcSymbol?.ReturnType ?? TypeSymbol.Object;
    }
    void TypeCheck(LocalVarDeclStatementAST decl, TypeCheckState tc)
    {
        var name = decl.Name.Name;
        bool hasExprChecked = false;
        TypeSymbol? declType;
        if (decl.DeclType is { } rawDeclType)
        {
            declType = tc.TypeFromTypeAST(rawDeclType, tc.SymbolTable);
        }
        else
        {
            // steal the type from the right hand side
            // note: this is not an error
            declType = null;
        }
        if (declType is null)
        {
            // stole the type from the right
            TypeCheck(decl.Expression, tc);
            declType = decl.Expression.Type;
            hasExprChecked = true;
        }
        if (tc.SymbolTable.ExistsAtCurrentLevel(name))
        {
            tc.SymbolAlreadyDefinedError(decl.Name);
        }
        else
        {
            tc.SymbolTable[name] = new LocalVarSymbol(declType);
        }
        TypeCheck(decl.Name, tc);
        if (!hasExprChecked)
            TypeCheck(decl.Expression, tc);
        tc.AssertTypeAssignableTo(decl.Expression, decl.Name.Type, decl.Expression.Type);
    }
    void TypeCheck(ConditionalControlFlowStatementAST controlFlowStmt, TypeCheckState tc)
    {
        if (controlFlowStmt.Condition is { } condExpr)
        {
            TypeCheck(condExpr, tc);
            tc.AssertTypeAssignableTo(condExpr, TypeSymbol.Boolean, condExpr.Type);
        }
        switch (controlFlowStmt)
        {
            case ExitStatementAST e:
                {
                    if (e.Label is { } lb)
                    {
                        if (!tc.ExitableLabels.Contains(lb.Name))
                        {
                            tc.TypeCheckError(lb, $"Label {lb.Name} is not marked on the exitable block in scope");
                        }
                    }
                    else if (!tc.CanExit)
                        tc.TypeCheckError(controlFlowStmt, "Cannot exit from here");
                }
                break;
            case BreakStatementAST b:
                {
                    if (b.Label is { } lb)
                    {
                        if (!tc.BreakableLabels.Contains(lb.Name))
                        {
                            tc.TypeCheckError(lb, $"Label {lb.Name} is not marked on the breakable block in scope");
                        }
                    }
                    else if (!tc.CanBreak)
                        tc.TypeCheckError(controlFlowStmt, "Cannot break from here");
                }
                break;
            case ContinueStatementAST c:
                {
                    if (c.Label is { } lb)
                    {
                        if (!tc.ContinuableLabels.Contains(lb.Name))
                        {
                            tc.TypeCheckError(lb, $"Label {lb.Name} is not marked on the continuable block in scope");
                        }
                    }
                    else if (!tc.CanContinue)
                        tc.TypeCheckError(controlFlowStmt, "Cannot continue from here");
                }
                break;
            case ReturnStatementAST r:
                if (r.ReturnValue is { } retValue)
                {
                    TypeCheck(retValue, tc);
                    if (tc.ExpectedReturnType is null)
                        tc.TypeCheckError(r, $"Expecting to return void, got {retValue.Type}");
                    else
                        tc.AssertTypeAssignableTo(retValue, tc.ExpectedReturnType, retValue.Type);
                }
                else
                {
                    if (tc.ExpectedReturnType is { } retType)
                        tc.TypeCheckError(r, $"Expecting to return {retType}, got no return value");
                }
                if (r.Condition is null)
                    // set that the function has returned for this path
                    tc.HasReturned.Value = true;
                // otherwise, we aren't sure if we will return or not
                break;
        }
    }
    void TypeCheck(IfStatementAST ifStmt, TypeCheckState tc)
    {
        // no matter if it is a do-while or while loop
        // the condition must be using variables in the outter scope.
        TypeCheck(ifStmt.Condition, tc);
        tc.AssertTypeAssignableTo(ifStmt.Condition, TypeSymbol.Boolean, ifStmt.Condition.Type);

        var childTc = tc with
        {
            CanExit = true,
            // if there's a return statement in for loop
            // we can't be sure that it will return
            // so we clone so child can't modify original value
            HasReturned = new(tc.HasReturned.Value),
            DefinedGotoLabels = new(tc.DefinedGotoLabels)
        };
        if (ifStmt.Label is { } lb)
        {
            if (!tc.DefinedGotoLabels.Add(lb.Name))
            {
                // label already present
                tc.TypeCheckError(lb, $"Label {lb.Name} is already present in the current scope.");
            }
            childTc.DefinedGotoLabels.Add(lb.Name);
            childTc = childTc with { ExitableLabels = CopyAndAdd(childTc.ExitableLabels, lb.Name) };
        }
        // make a clone of the current symbol table for the inner scope.
        TypeCheckBlock(tc.SymbolTable.Clone(), ifStmt.TrueBlock, childTc);
        TypeCheckBlock(tc.SymbolTable.Clone(), ifStmt.FalseBlock, childTc);
    }
    void TypeCheck(WhileStatementAST whileStmt, TypeCheckState tc)
    {
        // no matter if it is a do-while or while loop
        // the condition must be using variables in the outter scope.
        TypeCheck(whileStmt.Condition, tc);
        tc.AssertTypeAssignableTo(whileStmt.Condition, TypeSymbol.Boolean, whileStmt.Condition.Type);
        var childTc = tc with
        {
            CanExit = true,
            CanBreak = true,
            CanContinue = true,
            DefinedGotoLabels = new(tc.DefinedGotoLabels),
            // if there's a return statement in while loop
            // we can't be sure that it will return
            // so we clone so child can't modify original value
            HasReturned = new(tc.HasReturned.Value)
        };
        if (whileStmt.Label is { } lb)
        {
            if (!tc.DefinedGotoLabels.Add(lb.Name))
            {
                // label already present
                tc.TypeCheckError(lb, $"Label {lb.Name} is already present in the current scope.");
            }
            childTc.DefinedGotoLabels.Add(lb.Name);
            childTc = childTc with
            {
                BreakableLabels = CopyAndAdd(childTc.BreakableLabels, lb.Name),
                ContinuableLabels = CopyAndAdd(childTc.ContinuableLabels, lb.Name),
                ExitableLabels = CopyAndAdd(childTc.ExitableLabels, lb.Name),
            };
        }
        // make a clone of the current symbol table for the inner scope.
        TypeCheckBlock(tc.SymbolTable.Clone(), whileStmt.Block, childTc);
    }
    void TypeCheck(ForEachStatementAST foreachStmt, TypeCheckState tc)
    {
        // list type is not yet implemented so...
        if (foreachStmt.List is not BinaryAST binary || binary.Operator is not BinaryOperators.Range)
        {
            tc.TypeCheckError(foreachStmt.List, "Expecting a range in a for each loop");
        }
        // create child scope symbols
        var childScopeSymbols = tc.SymbolTable.Clone();
        // if it already exists, only allow if same type (check in TypeCheck below)
        // if not, declare a new one for the child
        if (!tc.SymbolTable.ExistsAtCurrentLevel(foreachStmt.Target.Name))
        {
            foreachStmt.UseOutterTarget = false;
            childScopeSymbols[foreachStmt.Target.Name] = new LocalVarSymbol(TypeSymbol.Int32);
        }
        else
        {
            foreachStmt.UseOutterTarget = true;
        }
        var childTc = tc with
        {
            SymbolTable = childScopeSymbols,
            CanExit = true,
            CanBreak = true,
            CanContinue = true,
            DefinedGotoLabels = new(tc.DefinedGotoLabels),
            // if there's a return statement in for loop
            // we can't be sure that it will return
            // so we clone so child can't modify original value
            HasReturned = new(tc.HasReturned.Value)
        };
        if (foreachStmt.Label is { } lb)
        {
            if (!tc.DefinedGotoLabels.Add(lb.Name))
            {
                // label already present
                tc.TypeCheckError(lb, $"Label {lb.Name} is already present in the current scope.");
            }
            childTc.DefinedGotoLabels.Add(lb.Name);
            childTc = childTc with
            {
                BreakableLabels = CopyAndAdd(childTc.BreakableLabels, lb.Name),
                ContinuableLabels = CopyAndAdd(childTc.ContinuableLabels, lb.Name),
                ExitableLabels = CopyAndAdd(childTc.ExitableLabels, lb.Name),
            };
        }
        // type check the target
        TypeCheck(foreachStmt.Target, childTc);
        tc.AssertTypeAssignableTo(foreachStmt.Target, TypeSymbol.Int32, foreachStmt.Target.Type);
        // type check the statements
        TypeCheck(foreachStmt.Block.Statements, childTc);
        // set the symbol table for the block
        foreachStmt.Block.InnerSymbols = childScopeSymbols;
    }
    void TypeCheck(LabelStatementAST labelStmt, TypeCheckState tc)
    {
        if (!tc.DefinedGotoLabels.Add(labelStmt.Label.Name))
        {
            tc.TypeCheckError(labelStmt.Label, "The label is already defined.");
        }
    }
    void TypeCheck(GotoStatementAST gotoStmt, TypeCheckState tc)
    {
        if (!tc.DefinedGotoLabels.Contains(gotoStmt.Label.Name))
        {
            tc.TypeCheckError(gotoStmt.Label, "The label is not found or is forward goto. (Forward goto is not yet implemented)");
        }
        if (gotoStmt.Condition is { } cond)
        {
            TypeCheck(cond, tc);
            tc.AssertTypeAssignableTo(gotoStmt.Condition, TypeSymbol.Boolean, cond.Type);
        }
    }
    void TypeCheckBlock(SymbolTable innerSymbolTable, BlockControlAST block, TypeCheckState tc)
    {
        // cached the current symbols
        var outterScopeSymbol = tc.SymbolTable;
        var labels = tc.DefinedGotoLabels;
        // sets the symbols to be the inner scope
        tc = tc with { SymbolTable = innerSymbolTable, DefinedGotoLabels = new HashSet<string>(labels) };
        // type checks
        TypeCheck(block.Statements, tc);
        // set the symbol table for the block
        block.InnerSymbols = innerSymbolTable;
    }
    void TypeCheck(ValueAST value, TypeCheckState tc)
    {
        switch (value)
        {
            case Int32ValueAST intValue:
                intValue.Type = TypeSymbol.Int32;
                break;
            case BooleanValueAST boolValue:
                boolValue.Type = TypeSymbol.Boolean;
                break;
            case StringValueAST stringValue:
                stringValue.Type = TypeSymbol.String;
                break;
            case ListDeclarationAST list:
                list.Type = TypeSymbol.List(TypeCheckLUB(list.Elements, tc));
                break;
            case ArrayDeclarationWithValuesAST arr:
                arr.Type = TypeSymbol.Array(TypeCheckLUB(arr.Elements, tc));
                break;
            default:
                tc.NotImplemented(value);
                break;
        }
    }
    TypeSymbol TypeCheckLUB(ListAST<ExpressionAST> expressions, TypeCheckState tc)
    {
        if (expressions.Count is 0) return TypeSymbol.Any;

        TypeCheck(expressions[0], tc);
        TypeSymbol type = expressions[0].Type;
        for (int i = 1; i < expressions.Count; i++)
        {
            TypeCheck(expressions[i], tc);
            type = tc.LUB(type, expressions[i].Type);
        }
        return type;
    }
    void TypeCheck(IdentifierAST id, TypeCheckState tc)
    {
        var varSymbol = tc.VarFromVarId(id, tc.SymbolTable);
        if (varSymbol is null)
        {
            id.Type = TypeSymbol.Object;
        }
        else
        {
            id.Type = varSymbol.Type;
        }
    }
    void TypeCheck(FunctionAST func, TypeCheckState tc)
    {
        var funcSymbol = tc.BuildFuncSymbol(func, tc.SymbolTable);


        if (tc.SymbolTable.ExistsAtCurrentLevel(func.Name.Name))
        {
            tc.SymbolAlreadyDefinedError(func.Name);
        }
        else
        {
            tc.SymbolTable[func.Name.Name] = funcSymbol;
        }
        var tcChild = new TypeCheckState(
            SymbolTable: func.SymbolTable,
            DefinedGotoLabels: [],
            CanExit: funcSymbol.ReturnType == TypeSymbol.Void, // only allow exit on void
            CanBreak: false,
            CanContinue: false,
            ExitableLabels: [],
            BreakableLabels: [],
            ContinuableLabels: [],
            HasReturned: new(false),
            ExpectedReturnType: funcSymbol.ReturnType
        );
        TypeCheck(func.Statements, tcChild);
        if (funcSymbol.ReturnType != TypeSymbol.Void && !tcChild.HasReturned.Value)
            tc.TypeCheckError(func.Name, "The function does not return on every path");
    }



}
record class TypeCheckState(
    SymbolTable SymbolTable,
    HashSet<string> DefinedGotoLabels,
    Ref<bool> HasReturned,
    bool CanExit,
    bool CanBreak,
    bool CanContinue,
    HashSet<string> ExitableLabels,
    HashSet<string> BreakableLabels,
    HashSet<string> ContinuableLabels,
    TypeSymbol? ExpectedReturnType = null
) : ITypeCheckErrorState;
interface ITypeCheckErrorState
{

}
class Ref<T>(T defaultValue = default) where T : struct
{
    public T Value { get; set; } = defaultValue;
}