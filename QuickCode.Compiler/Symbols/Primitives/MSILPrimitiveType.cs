using QuickCode.Symbols;
using QuickCode.Symbols.SymbolTables;
using Mono.Cecil;

namespace QuickCode.Compiler.Symbols.Primitives;

abstract class MSILPrimitiveType : INativeTypeSymbol
{
    public MSILPrimitiveType(TypeReference typeReference)
    {
        TypeReference = typeReference;
    }

    public TypeReference TypeReference { get; }
    public abstract ITypeSymbol BaseType { get; }
    public abstract IFieldSymbolTable Fields { get; }
    public abstract IFuncSymbolTable Functions { get; }
    public abstract IConstructorSymbolTable Constructors { get; }
    public abstract ITypeSymbolTable Types { get; }
}
