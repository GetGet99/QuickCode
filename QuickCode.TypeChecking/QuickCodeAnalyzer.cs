using QuickCode.AST;
using QuickCode.AST.Classes;
using QuickCode.AST.FileProgram;
using QuickCode.AST.TopLevels;
using System.Diagnostics;
using QuickCode.Symbols;
using QuickCode.Symbols.Compiler.Implementation;
using QuickCode.Symbols.Factories;
using QuickCode.Symbols.SymbolTables;

namespace QuickCode.TypeChecking;

public static class QuickCodeAnalyzer
{
    public static void Analyze(
        TopLevelQuickCodeProgramAST? toplevel,
        IEnumerable<QuickCodeFileProgramAST> files,
        GlobalSymbols globalSymbols,
        IScopeSymbolTable globalConstantSymbolTables,
        ITypeFactory typeFactory
    )
    {
        TypeAnalyzerState ta = new(globalSymbols, globalConstantSymbolTables, null, globalSymbols, typeFactory, []);
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
        // PHASE 3: NATIVE MAPPING
        var symMapper = new SymbolMapper(ta);
        foreach (var (type, nativeType) in ta.NativeMapping)
        {
            ta.TypeFactory.Fill(nativeType, type, symMapper);
        }
    }
    class SymbolMapper(TypeAnalyzerState ta) : ITypeSymbolMapper
    {
        public INativeTypeSymbol this[IUserTypeSymbol userType] => ta.NativeMapping[userType];
    }
    static void AnalyzePhase1(QuickCodeNamespaceAST ns, TypeAnalyzerState ta)
    {
        var dns = ta.DeclaredNamespace.TryGetOrCreateNamespace(
            string.Join('.', from n in ns.Name select n.Name)
        );
        if (dns is null)
        {
            ta.TypeCheckError(ns, "The current name already exists as something other than namespace.");
            // error handling not implemented
            ta.NotImplemented(ns);
            return;
        }
        ta = ta with { DeclaredNamespace = dns };
        foreach (var cls in ns.Types)
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
        var newType = new QuickCodeTypeSymbol(ta.DeclaredNamespace.AppendNamespaceTo(typeName), ta.TypeFactory.Object);
        ta.DeclaredNamespace[typeName] = newType;
        ta.NativeMapping[newType] = ta.TypeFactory.Declare(ta.DeclaredNamespace.AppendNamespaceTo(typeName));
    }
    static void AnalyzePhase2(QuickCodeNamespaceAST ns, TypeAnalyzerState ta)
    {
        var dns = ta.DeclaredNamespace.TryGetOrCreateNamespace(
            string.Join('.', from n in ns.Name select n.Name)
        );
        if (dns is null)
        {
            ta.TypeCheckError(ns, "The current name already exists as something other than namespace.");
            // error handling not implemented
            ta.NotImplemented(ns);
            return;
        }
        ta = ta with { DeclaredNamespace = dns };
        foreach (var cls in ns.Types)
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
        if (ta.DeclaredNamespace[typeName] is not IUserTypeSymbol newType)
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
        else if (ta.CurrentType.Fields.GetInCurrentLevel(fieldName) is not null)
        {
            error = true;
            ta.SymbolAlreadyDefinedError(fieldDeclStatement.Name);
        }
        else if (ta.CurrentType.Functions.ContainsFunctionInCurrentLevel(fieldName))
        {
            error = true;
            ta.SymbolAlreadyDefinedError(fieldDeclStatement.Name);
        }
        if (fieldDeclStatement.DeclType is null)
        {
            // currently, implicit type is not yet implemented
            ta.NotImplemented(fieldDeclStatement);
        }
        var type = ta.TypeFromTypeAST(fieldDeclStatement.DeclType, ta.CurrentScopeSymbol.Types);
        if (error || type is null)
        {
            return;
        }
        if (ta.CurrentType.Fields is not IFieldSymbolTableWritable fieldsTable)
        {
            ta.NotImplemented(fieldDeclStatement);
            return;
        }

        fieldsTable[fieldName] = new QuickCodeFieldSymbol(type, fieldName);
    }
    static void AnalyzePhase2(FunctionAST functionAST, TypeAnalyzerState ta)
    {
        ArgumentNullException.ThrowIfNull(ta.CurrentType);

        var funcName = functionAST.Name.Name;
        bool error = false;
        if (ta.CurrentType.Fields.GetInCurrentLevel(funcName) is not null)
        {
            error = true;
            ta.SymbolAlreadyDefinedError(functionAST.Name);
        }
        if (ta.CurrentType.Functions is not IFuncSymbolTableWritable funcSymbolTableWritable)
        {
            ta.NotImplemented(functionAST);
            return;
        }

        if (error)
        {
            return;
        }

        var (userfunc, argsInfo) = ta.BuildFuncSymbol(
            functionAST,
            ta.CurrentScopeSymbol,
            ImplicitThisArgumentType: ta.CurrentType
        );

        funcSymbolTableWritable.AddStatic(funcName, userfunc);
    }
}
record class TypeAnalyzerState(
    GlobalSymbols GlobalSymbols,
    IScopeSymbolTable CurrentScopeSymbol,
    IUserTypeSymbol? CurrentType,
    INamespaceSymbol DeclaredNamespace,
    ITypeFactory TypeFactory,
    Dictionary<IUserTypeSymbol, INativeTypeSymbol> NativeMapping
) : ITypeCheckErrorState;
