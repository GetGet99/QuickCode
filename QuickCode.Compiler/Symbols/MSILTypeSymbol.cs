using Mono.Cecil;
using QuickCode.Symbols;
using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Compiler.Symbols;

class MSILTypeSymbol : INativeTypeSymbol
{
    public MSILTypeSymbol(MSILTypeFactory factory, TypeReference typeReference)
    {
        if (typeReference == factory.MSILTypeSystem.Object || typeReference == factory.MSILTypeSystem.Void)
        {
            BaseType = this;
        }
        else
        {
            BaseType = factory.ResolveType(typeReference.Resolve().BaseType);
        }
    }
    public ITypeSymbol BaseType { get; }

    public IFieldSymbolTable Fields => throw new NotImplementedException();

    public IFuncSymbolTable Functions => throw new NotImplementedException();

    public IConstructorSymbolTable Constructors => throw new NotImplementedException();

    public ITypeSymbolTable Types => throw new NotImplementedException();
}
