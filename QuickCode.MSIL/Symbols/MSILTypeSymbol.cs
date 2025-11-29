using Mono.Cecil;
using Mono.Cecil.Rocks;
using QuickCode.Symbols;
using QuickCode.Symbols.Compiler.Implementation;
using QuickCode.Symbols.Functions;
using QuickCode.Symbols.Operators;
using QuickCode.Symbols.SymbolTables;
namespace QuickCode.MSIL.Symbols;
record class MSILTypeSymbol(TypeReference MSILType) : INativeTypeSymbol, IFieldSymbolTable, IFuncSymbolTable, IConstructorSymbolTable
{
    ITypeSymbol ITypeSymbol.BaseType => MSILType.Resolve().BaseType.Symbol();
    IFieldSymbol? IFieldSymbolTable.this[string name]
    {
        get
        {
            var item = MSILType.Resolve().Fields.FirstOrDefault(x => x.Name == name);
            if (item is null) return null;
            return new MSILFieldSymbol(item);
        }
    }
    IEnumerable<IFieldSymbol> IFieldSymbolTable.CurrentLevel
    {
        get
        {
            foreach (var field in MSILType.Resolve().Fields)
            {
                yield return field.Symbol();
            }
        }
    }

    // IFieldSymbolTable.GetInCurrentLevel implementation
    IFieldSymbol? IFieldSymbolTable.GetInCurrentLevel(string name)
    {
        var field = MSILType.Resolve().Fields.FirstOrDefault(f => f.Name == name);
        return field is null ? null : field.Symbol();
    }

    public IFuncSymbol? this[string name, ArgumentInfo[] args]
        => OverloadResolvation.ResolveBestMethod(args, GetForName(name).Select(x => new MSILFuncSymbol(x)));

    IEnumerable<IFuncSymbol> IFuncSymbolTable.CurrentLevel
    {
        get
        {
            foreach (var func in MSILType.Resolve().Methods)
            {
                if (!func.IsHideBySig)
                    continue;
                yield return func.SymbolFunc();
            }
        }
    }

    // IFuncSymbolTable.ContainsFunctionInCurrentLevel implementation
    bool IFuncSymbolTable.ContainsFunctionInCurrentLevel(string name)
    {
        var typeDef = MSILType.Resolve();
        return typeDef.Methods.Any(m => m.IsHideBySig && m.Name == name);
    }

    public IUnaryOperatorFuncSymbol? this[UnaryOperators unary, ITypeSymbol target] // Fix for CS0738
        => OverloadResolvation.ResolveBestMethod<IUnaryOperatorFuncSymbol>(
            target,
            GetForName(MSILOperatorMap.ToName(unary), hideBySig: true, specialName: true).Select(x => new MSILUnaryFuncSymbol(x)
        ));

    IEnumerable<IBinaryOperatorFuncSymbol> IFuncSymbolTable.CurrentLevelBinaryOperators
    {
        get
        {
            foreach (var func in MSILType.Resolve().Methods)
            {
                if (!func.IsHideBySig)
                    continue;
                if (!func.IsSpecialName)
                    continue;
                if (MSILOperatorMap.TryToBianryOperator(func.Name, out var op))
                    yield return func.SymbolBinary();
            }
        }
    }

    IEnumerable<IUnaryOperatorFuncSymbol> IFuncSymbolTable.CurrentLevelUnaryOperators
    {
        get
        {
            var typeDef = MSILType.Resolve();
            foreach (var method in typeDef.Methods)
            {
                if (!method.IsHideBySig || !method.IsSpecialName)
                    continue;
                if (MSILOperatorMap.TryToUnaryOperator(method.Name, out var op))
                    yield return method.SymbolUnary();
            }
        }
    }

    IEnumerable<IUnaryWriteOperatorFuncSymbol> IFuncSymbolTable.CurrentLevelUnaryWriteOperators
    {
        get
        {
            var typeDef = MSILType.Resolve();
            foreach (var method in typeDef.Methods)
            {
                if (!method.IsHideBySig || !method.IsSpecialName)
                    continue;
                if (MSILOperatorMap.TryToUnaryWriteOpeartorBefore(method.Name, out var op))
                {
                    yield return method.SymbolUnaryWrite(before: true);
                    yield return method.SymbolUnaryWrite(before: false);
                }
            }
        }
    }

    public IUnaryWriteOperatorFuncSymbol? this[UnaryWriteOperators unaryWrite, ITypeSymbol target]
        => OverloadResolvation.ResolveBestMethod(
            target,
            GetForName(MSILOperatorMap.ToName(unaryWrite), hideBySig: true, specialName: true).Select(x => new MSILUnaryWriteFuncSymbol(x, unaryWrite is UnaryWriteOperators.IncrementBefore or UnaryWriteOperators.DecrementBefore)
        ));

    IEnumerable<MethodDefinition> GetForName(string name, bool hideBySig = false, bool specialName = false)
    {
        var typeRef = MSILType;
        while (typeRef != null)
        {
            var typeDef = typeRef.Resolve();
            foreach (var method in typeDef.Methods)
            {
                if (method.IsHideBySig == hideBySig && method.IsSpecialName == specialName && method.Name == name)
                    yield return method;
            }
            typeRef = typeDef.BaseType;
        }
    }

    public IBinaryOperatorFuncSymbol? this[BinaryOperators binary, ITypeSymbol left, ITypeSymbol right]
    {
        get
        {
            var name = MSILOperatorMap.ToName(binary);
            if (name is null)
                return null;
            return OverloadResolvation.ResolveBestMethod(left, right, GetForName(name, hideBySig: true, specialName: true).Select(x => new MSILBinaryFuncSymbol(x)));
        }
    }

    IConstructorSymbol? IConstructorSymbolTable.this[ArgumentInfo[] args]
        => OverloadResolvation.ResolveBestMethod(args, MSILType.Resolve().GetConstructors().Select(x => new MSILConstructorSymbol(x)));

    IEnumerable<IConstructorSymbol> IConstructorSymbolTable.Constructors
    {
        get
        {
            var typeDef = MSILType.Resolve();
            foreach (var ctor in typeDef.GetConstructors())
            {
                yield return new MSILConstructorSymbol(ctor);
            }
        }
    }

    public IFieldSymbolTable Fields => this;

    public IFuncSymbolTable Functions => this;

    public IConstructorSymbolTable Constructors => this;

    public ITypeSymbolTable Types => throw new NotImplementedException();
}
