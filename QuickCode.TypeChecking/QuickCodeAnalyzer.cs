using QuickCode.AST;
using QuickCode.AST.Classes;
using QuickCode.AST.FileProgram;
using QuickCode.AST.Symbols;
using QuickCode.AST.TopLevels;
using System.Diagnostics;

namespace QuickCode.TypeChecking;
using static TypeCheckHelper;

public class QuickCodeAnalyzer
{
    public static void Analyze(
        TopLevelQuickCodeProgramAST? toplevel,
        IEnumerable<QuickCodeFileProgramAST> files,
        GlobalSymbols globalSymbols,
        SymbolTable globalConstantSymbolTables
    )
    {
        TypeAnalyzerState ta = new(globalSymbols, globalConstantSymbolTables, null, globalSymbols);
        // PHASE 1: TYPES
        foreach (var file in files)
        {
            foreach (var ns in file.Namespaces)
                AnalyzePhase1(ns, ta);
        }
        // PHASE 2: MEMBERS (requires TYPES to be defined)
        foreach (var file in files)
        {
            foreach (var ns in file.Namespaces)
                AnalyzePhase2(ns, ta);
        }
    }
    static void AnalyzePhase1(QuickCodeNamespaceAST ns, TypeAnalyzerState ta)
    {
        foreach (var cls in ns.Classes)
            AnalyzePhase1(cls, ta);
    }
    static void AnalyzePhase1(QuickCodeClassAST cls, TypeAnalyzerState ta)
    {
        if (cls.Name is not TypeIdentifierAST idt)
        {
            // generic types, etc.
            ta.NotImplemented(cls);
            return;
        }
        var typeName = idt.Name.Name;
        var newType = new SimpleTypeSymbol(ta.DeclaredNamespace.AppendNamespaceTo(typeName));
        ta.DeclaredNamespace[typeName] = newType;
    }
    static void AnalyzePhase2(QuickCodeNamespaceAST ns, TypeAnalyzerState ta)
    {
        foreach (var cls in ns.Classes)
            AnalyzePhase2(cls, ta);
    }
    static void AnalyzePhase2(QuickCodeClassAST cls, TypeAnalyzerState ta)
    {
        if (cls.Name is not TypeIdentifierAST idt)
        {
            // generic types, etc.
            ta.NotImplemented(cls);
            return;
        }
        var typeName = idt.Name.Name;
        if (ta.DeclaredNamespace[typeName] is not TypeSymbol newType)
        {
            throw new UnreachableException();
        }
        ta = ta with { CurrentType = newType };
        foreach (var decl in cls.Declarables)
        {
            switch (decl)
            {
                case FieldDeclStatementAST fieldDeclStatement:
                    AnalyzePhase2(fieldDeclStatement, ta);
                    break;
                case FunctionAST functionDeclStatement:
                    AnalyzePhase2(functionDeclStatement, ta);
                    break;
                default:
                    ta.NotImplemented((AST.AST)decl);
                    break;
            }
        }
    }
    static void AnalyzePhase2(FieldDeclStatementAST fieldDeclStatement, TypeAnalyzerState ta)
    {
        ArgumentNullException.ThrowIfNull(ta.CurrentType);

        var fieldName = fieldDeclStatement.Name.Name;
        bool error = false;
        if (fieldName is "this")
        {
            // we don't allow this to be defined.
            error = true;
            ta.SymbolAlreadyDefinedError(fieldDeclStatement.Name);
        }
        else if (ta.CurrentType.Children.ExistsAtCurrentLevel(fieldName))
        {
            error = true;
            ta.SymbolAlreadyDefinedError(fieldDeclStatement.Name);
        }
        if (fieldDeclStatement.DeclType is null)
        {
            // currently, implicit type is not yet implemented
            ta.NotImplemented(fieldDeclStatement);
        }
        var type = ta.TypeFromTypeAST(fieldDeclStatement.DeclType, ta.CurrentScopeSymbol);
        if (error || type is null)
        {
            return;
        }

        ta.CurrentType.Children[fieldName] = new FieldSymbol(
            type,
            fieldName
        );
    }
    static void AnalyzePhase2(FunctionAST functionAST, TypeAnalyzerState ta)
    {
        ArgumentNullException.ThrowIfNull(ta.CurrentType);

        var funcName = functionAST.Name.Name;
        bool error = false;
        if (ta.CurrentType.Children.ExistsAtCurrentLevel(funcName))
        {
            error = true;
            ta.SymbolAlreadyDefinedError(functionAST.Name);
        }

        if (error)
        {
            return;
        }

        var userfunc = ta.BuildFuncSymbol(
            functionAST,
            ta.CurrentScopeSymbol,
            ImplicitThisArgumentType: ta.CurrentType
        );

        ta.CurrentType.Children[funcName] = userfunc;
    }
}
record class TypeAnalyzerState(GlobalSymbols GlobalSymbols, SymbolTable CurrentScopeSymbol, TypeSymbol? CurrentType, INamespaceSymbol DeclaredNamespace) : ITypeCheckErrorState;
