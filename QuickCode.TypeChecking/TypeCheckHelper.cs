using QuickCode.AST;
using QuickCode.AST.Expressions;
using QuickCode.Symbols;
using QuickCode.Symbols.Compiler;
using QuickCode.Symbols.Compiler.Implementation;
using QuickCode.Symbols.Functions;
using QuickCode.Symbols.SymbolTables;
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
    public static void SymbolUndefinedTypeError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"Type {node.Name} is not defined in the current scope.");
    public static void SymbolUndefinedFunctionError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"Function {node.Name} is not defined in the current scope.");
    public static void SymbolAlreadyDefinedError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"{node.Name} is already declared in the current scope.");
    public static void SymbolIsNotTypeError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"{node.Name} is not a type.");
    public static void NoProperOverloadedError(this ITypeCheckErrorState s, IOverloadable node)
        => s.TypeCheckError((AST.AST)node, $"Functions {node.Name} has no overload that can accept the given types.");
    public static void SymbolIsNotFunctionError(this ITypeCheckErrorState s, IdentifierAST node)
        => s.TypeCheckError(node, $"{node.Name} is not a function.");
    public static void TypeMismatchError(this ITypeCheckErrorState s, AST.AST node, ITypeSymbol expected, ITypeSymbol actual)
        => s.TypeCheckError(node, $"Expected {expected} but got {actual}");
    public static bool AssertTypeAssignableTo(this ITypeCheckErrorState s, AST.AST node, ITypeSymbol expected, ITypeSymbol actual)
    {
        bool isEqual = expected == actual || (expected == s.TypeFactory.Object && actual != s.TypeFactory.Void);
        if (!isEqual)
        {
            s.TypeMismatchError(node, expected, actual);
        }
        return isEqual;
    }
    public static ITypeSymbol? TypeFromTypeAST(this ITypeCheckErrorState s, TypeAST typeAST, ITypeSymbolTable symbol)
    {
        if (typeAST is not TypeIdentifierAST typeIdentifier)
        {
            if (typeAST is CompositeTypeAST compositeTypeAST)
            {
                var targs = new ITypeSymbol?[compositeTypeAST.TypeArguments.Count];
                bool exit = false;
                for (int i = 0; i < compositeTypeAST.TypeArguments.Count; i++)
                {
                    targs[i] = s.TypeFromTypeAST(compositeTypeAST.TypeArguments[i], symbol);
                    if (targs[i] is null)
                    {
                        exit = true;
                    }
                }
                if (exit) return null;
                var t1 = symbol[compositeTypeAST.Name.Name, targs!];
                if (t1 is null)
                {
                    // TODO: Error message
                    s.NotImplemented(compositeTypeAST);
                }
                return t1;
            }
            s.NotImplemented(typeAST);
            return null;
        }
        var type = symbol[typeIdentifier.Name.Name, []];
        if (type is null)
        {
            s.SymbolUndefinedTypeError(typeIdentifier.Name);
            return null;
        }
        return type;
    }
    public static VarSymbol? VarFromVarId(this ITypeCheckErrorState tc, IdentifierAST varId, IVariableSymbolTableWritable symbol, bool silentIfNotVariable = false)
    {
        VarSymbol? varSym;
        var declSymbol = symbol.GetSymbol(varId.Name, out var curLevel);
        if (declSymbol is null)
        {
            if (!silentIfNotVariable)
                tc.SymbolUndefinedError(varId);
            varSym = null;
        }
        else if (declSymbol is not VarSymbol variable)
        {
            if (!silentIfNotVariable) 
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
    public static ITypeSymbol LUB(this ITypeCheckErrorState tc, ITypeSymbol t1, ITypeSymbol t2)
    {
        if (t1 != t2)
        {
            // TODO
            return tc.TypeFactory.Object;
        }
        else
        {
            return t1;
        }
    }
    /// <param name="ImplicitThisArgumentType">If null, this is a static method. Otherwise, it is not a static method, and `this` is of this type</param>
    public static (QuickCodeFuncSymbol, ArgumentInfo[] args) BuildFuncSymbol(this ITypeCheckErrorState tc, FunctionAST func, IScopeSymbolTable curSymbol, ITypeSymbol? ImplicitThisArgumentType = null)
    {
        var childSymbol = new QuickCodeScopeSymbolTable(curSymbol);
        var Parameters = new ParameterInfo[ImplicitThisArgumentType is not null ? (func.Parameters.Count + 1) : func.Parameters.Count];
        int paramIdx = 0;
        if (ImplicitThisArgumentType is not null)
        {
            Parameters[paramIdx++] = new("this", ImplicitThisArgumentType, false, null);
        }
        for (int i = 0; i < func.Parameters.Count; i++)
        {
            var param = func.Parameters[i];
            var type = tc.TypeFromTypeAST(param.Type, curSymbol.Types) ?? tc.TypeFactory.Object;
            Parameters[paramIdx++] = new(param.Name.Name, type, false, null);
            childSymbol.Variables[param.Name.Name] = new ParameterVarSymbol(i, type);
        }
        func.SymbolTable = childSymbol;
        ITypeSymbol? ReturnType;
        if (func.ReturnType is null)
            ReturnType = null;
        else
            ReturnType = tc.TypeFromTypeAST(func.ReturnType, curSymbol.Types) ?? tc.TypeFactory.Object;
        return (func.FuncSymbol = new QuickCodeFuncSymbol(func, func.Name.Name, tc.CurrentType, [.. Parameters], ReturnType ?? tc.TypeFactory.Void), [.. Parameters.Select(x => new ArgumentInfo(x.Name, x.Type))]);
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
    public static INativeTypeSymbol GetNative(this TypeCheckState tc, ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INativeTypeSymbol nativeType)
        {
            return nativeType;
        }
        if (tc.NativeSymbolsMapping.TryGetValue(typeSymbol, out var native))
        {
            return native;
        }
        throw new InvalidOperationException($"Type {typeSymbol} is not a native type and has no mapping in NativeSymbolsMapping.");
    }
}
