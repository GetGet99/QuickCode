using Mono.Cecil;
using QuickCode.Compiler.Symbols.Primitives;
using QuickCode.Symbols;
using QuickCode.Symbols.Factories;

namespace QuickCode.Compiler.Symbols;

public class MSILTypeFactory(ModuleDefinition moduleDefinition) : ITypeFactory
{
    public TypeSystem MSILTypeSystem => moduleDefinition.TypeSystem;
    public INativeTypeSymbol Boolean => field ??= new MSILBool(this);
    public INativeTypeSymbol Int32 => field ??= new MSILInt32(this);
    public INativeTypeSymbol Object => field ??= new MSILTypeSymbol(this, moduleDefinition.TypeSystem.Object);
    public INativeTypeSymbol Void => field ??= new MSILTypeSymbol(this, moduleDefinition.TypeSystem.Void);
    public INativeTypeSymbol String => field ??= new MSILString(this);
    public INativeTypeSymbol Array(INativeTypeSymbol ofType) => throw new NotImplementedException();
    public INativeTypeSymbol List(INativeTypeSymbol ofType) => throw new NotImplementedException();
    public INativeTypeSymbol CreateGeneric(INativeTypeSymbol type, INativeTypeSymbol[] typeParameters) => throw new NotImplementedException();
    public INativeTypeSymbol Declare(string name) => throw new NotImplementedException();
    public void Fill(INativeTypeSymbol nativeType, IUserTypeSymbol userType, ITypeSymbolMapper mapper) => throw new NotImplementedException();
    readonly Dictionary<string, INativeTypeSymbol> cache = [];
    public INativeTypeSymbol ResolveType(TypeReference typeReference)
    {
        if (cache.TryGetValue(typeReference.FullName, out var result))
        {
            return result;
        }
        if (typeReference.FullName == MSILTypeSystem.String.FullName)
        {
            return cache[typeReference.FullName] = String;
        }
        if (typeReference.FullName == MSILTypeSystem.Boolean.FullName)
        {
            return cache[typeReference.FullName] = Boolean;
        }
        if (typeReference.FullName == MSILTypeSystem.Int32.FullName)
        {
            return cache[typeReference.FullName] = Int32;
        }
        if (typeReference.FullName == MSILTypeSystem.Object.FullName)
        {
            return cache[typeReference.FullName] = Object;
        }
        if (typeReference.FullName == MSILTypeSystem.Void.FullName)
        {
            return cache[typeReference.FullName] = Void;
        }
        throw new NotImplementedException();
        //return cache[typeReference.FullName] = new MSILTypeSymbol(this, typeReference);
    }
}