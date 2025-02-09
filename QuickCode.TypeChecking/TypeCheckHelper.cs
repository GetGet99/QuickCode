using QuickCode.AST;
using QuickCode.AST.Expressions;
using QuickCode.AST.Symbols;
using System;
using System.Diagnostics.CodeAnalysis;

namespace QuickCode.TypeChecking;

internal static class TypeCheckHelper
{
    [DoesNotReturn]
    public static void NotImplemented(this ITypeCheckErrorState s, AST.AST node)
        => throw new NotImplementedException($"The node of type {node.GetType().Name} is not implemented.");
    /// <summary>
    /// Reports the error.
    /// </summary>
    /// <param name="node">The node to report the error to</param>
    /// <param name="error">The error to be reported.</param>
    public static void TypeCheckError(this ITypeCheckErrorState s, AST.AST node, string error)
        => throw new NotImplementedException($"{node} type check error: {error}");
    public static void SymbolUndefinedError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"{node.Name} is not defined in the current scope.");
    public static void SymbolAlreadyDefinedError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"{node.Name} is already declared in the current scope.");
    public static void SymbolIsNotTypeError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"{node.Name} is not a type.");
    public static void SymbolIsNotFunctionError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"{node.Name} is not a function.");
    public static void TypeMismatchError(this ITypeCheckErrorState s, AST.AST node, TypeSymbol expected, TypeSymbol actual)
        => s.TypeCheckError(node, $"Expected {expected.Name} but got {actual.Name}");
    public static bool AssertTypeAssignableTo(this ITypeCheckErrorState s, AST.AST node, TypeSymbol expected, TypeSymbol actual)
    {
        bool isEqual = expected == actual || (expected == TypeSymbol.Object && actual != TypeSymbol.Void);
        if (!isEqual)
        {
            s.TypeMismatchError(node, expected, actual);
        }
        return isEqual;
    }
    public static TypeSymbol? TypeFromTypeAST(this ITypeCheckErrorState s, TypeAST typeAST, SymbolTable symbol)
    {
        if (typeAST is not TypeIdentifierAST typeIdentifier)
        {
            if (typeAST is CompositeTypeAST compositeTypeAST)
            {
                var t1 = TypeFromTypeId(compositeTypeAST.Name, symbol);
                var targs =
                (
                    from t in compositeTypeAST.TypeArguments
                    select s.TypeFromTypeAST(t, symbol)
                ).ToArray();
                if (t1 is not SimpleTypeSymbol sts) return null;
                if (targs.Any(x => x is null)) return null;
                return new CompositeTypeSymbol(sts, targs);
            }
            s.NotImplemented(typeAST);
            return null;
        }
        return TypeFromTypeId(typeIdentifier.Name, symbol);

        TypeSymbol? TypeFromTypeId(IdentifierAST typeId, SymbolTable symbol)
        {
            var type = symbol[typeId.Name];
            if (type is null)
            {
                s.SymbolUndefinedError(typeId);
                return null;

            }
            else if (type is not TypeSymbol typeSym)
            {
                s.SymbolIsNotTypeError(typeId);
                return null;
            }
            else
            {
                return typeSym;
            }
        }
    }
    public static VarSymbol? VarFromVarId(this ITypeCheckErrorState tc, IdentifierAST varId, SymbolTable symbol)
    {
        VarSymbol? varSym;
        var declSymbol = symbol.GetSymbol(varId.Name, out var curLevel);
        if (declSymbol is null)
        {
            tc.SymbolUndefinedError(varId);
            varSym = null;
        }
        else if (declSymbol is not VarSymbol variable)
        {
            tc.TypeCheckError(varId, $"The symbol {varId.Name} is not a variable.");
            varSym = null;
        }
        else
        {
            varSym = variable;
            if (!curLevel && variable is LocalVarSymbol localVar)
            {
                localVar.HasChildFunctionAccess = true;
            }
        }
        return varSym;
    }
    public static TypeSymbol LUB(this ITypeCheckErrorState tc, TypeSymbol t1, TypeSymbol t2)
    {
        if (t1 != t2)
        {
            return TypeSymbol.Object;
        }
        else
        {
            return t1;
        }
    }
    /// <param name="ImplicitThisArgumentType">If null, this is a static method. Otherwise, it is not a static method, and `this` is of this type</param>
    public static UserFuncSymbol BuildFuncSymbol(this ITypeCheckErrorState tc, FunctionAST func, SymbolTable curSymbol, TypeSymbol? ImplicitThisArgumentType = null)
    {
        var name = func.Name.Name;
        var childSymbol = new SymbolTable(curSymbol);
        List<(TypeSymbol Type, string Name)> Parameters = [];
        if (ImplicitThisArgumentType is not null)
        {
            Parameters.Add((ImplicitThisArgumentType, "this"));
        }
        for (int i = 0; i < func.Parameters.Count; i++)
        {
            var param = func.Parameters[i];
            var type = tc.TypeFromTypeAST(param.Type, curSymbol) ?? TypeSymbol.Object;
            Parameters.Add((type, param.Name.Name));
            childSymbol[param.Name.Name] = new ParameterVarSymbol(i, type);
        }
        func.SymbolTable = childSymbol;
        TypeSymbol ReturnType;
        if (func.ReturnType is null)
            ReturnType = TypeSymbol.Void;
        else
            ReturnType = tc.TypeFromTypeAST(func.ReturnType, curSymbol) ?? TypeSymbol.Object;
        return new UserFuncSymbol(func, [.. Parameters], ReturnType);
    }
    /// <summary>
    /// Basically the "and" operator, but if a is false, early shortcut is not taken.<br/>
    /// (b is also evaluated no matter what)
    /// </summary>
    /// <returns>The result of a and b, with both expressions evaluated.</returns>
    public static bool Combine(bool a, bool b)
    {
        if (a)
            return true;
        if (b)
            return true;
        return false;
    }
    public static HashSet<T> CopyAndAdd<T>(HashSet<T> values, T value)
    {
        var newSet = new HashSet<T>(values) { value };
        return newSet;
    }
}
