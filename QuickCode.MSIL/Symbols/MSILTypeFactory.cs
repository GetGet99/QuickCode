using Mono.Cecil;
using Mono.Cecil.Rocks;
using QuickCode.Symbols;
using QuickCode.Symbols.Factories;

namespace QuickCode.MSIL.Symbols;

public class MSILTypeFactory(TypeSystem typeSystem, TypeDefinition listType, ModuleDefinition UserModule) : ITypeFactory
{
    public INativeTypeSymbol Boolean => typeSystem.Boolean.Resolve().Symbol();
    public INativeTypeSymbol String => typeSystem.String.Resolve().Symbol();
    public INativeTypeSymbol Int32 => typeSystem.Int32.Resolve().Symbol();
    public INativeTypeSymbol Object => typeSystem.Object.Resolve().Symbol();
    public INativeTypeSymbol Void => typeSystem.Void.Resolve().Symbol();
    public INativeTypeSymbol Array(INativeTypeSymbol ofType)
        => ofType.MSIL().MakeArrayType().Resolve().Symbol();

    public INativeTypeSymbol CreateGeneric(INativeTypeSymbol type, INativeTypeSymbol[] typeParameters)
    {
        return type.MSIL().MakeGenericInstanceType(
            [.. from t in typeParameters select t.MSIL()]
        ).Resolve().Symbol();
    }

    public INativeTypeSymbol List(INativeTypeSymbol ofType)
    {
        return listType.MakeGenericInstanceType(ofType.MSIL()).Resolve().Symbol();
    }
    public INativeTypeSymbol Declare(INamespaceSymbol ns, string name)
    {
        var type = new TypeDefinition(ns.FullName, name, TypeAttributes.Class);
        return type.Symbol();
    }
    public void Fill(INativeTypeSymbol ourType, IUserTypeSymbol userType, ITypeSymbolMapper mapper)
    {
        var type = ourType.MSIL().Resolve();
        TypeReference Resolve(ITypeSymbol type)
        {
            if (type is INativeTypeSymbol ourType)
                return ourType.MSIL();
            if (type is IUserTypeSymbol userFieldType)
                return mapper[userFieldType].MSIL();
            throw new ArgumentException("Type must be either native or user type");
        }
        foreach (var userField in userType.Fields.CurrentLevel)
        {
            type.Fields.Add(new FieldDefinition(userField.Name, FieldAttributes.Private, Resolve(userField.Type)));
        }
        foreach (var userFunc in userType.Functions.CurrentLevel)
        {
            var method = new MethodDefinition(userFunc.Name, MethodAttributes.Public, Resolve(userFunc.ReturnType));
            foreach (var param in userFunc.Parameters)
            {
                method.Parameters.Add(new(param.Name, ParameterAttributes.None, Resolve(param.Type)));
            }
            type.Methods.Add(method);
        }
        foreach (var userFunc in userType.Functions.CurrentLevelBinaryOperators)
        {
            var method = new MethodDefinition(
                MSILOperatorMap.ToName(userFunc.Operator),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                Resolve(userFunc.ReturnType)
            );
            method.Parameters.Add(new(Resolve(userFunc.LeftInputType)));
            method.Parameters.Add(new(Resolve(userFunc.RightInputType)));
            type.Methods.Add(method);
        }
        foreach (var userFunc in userType.Functions.CurrentLevelUnaryOperators)
        {
            var method = new MethodDefinition(
                MSILOperatorMap.ToName(userFunc.Operator),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                Resolve(userFunc.ReturnType)
            );
            method.Parameters.Add(new(Resolve(userFunc.InputType)));
            type.Methods.Add(method);
        }
        foreach (var userFunc in userType.Functions.CurrentLevelUnaryWriteOperators)
        {
            var method = new MethodDefinition(
                MSILOperatorMap.ToName(userFunc.Operator),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                Resolve(userFunc.ReturnType)
            );
            method.Parameters.Add(new(Resolve(userFunc.InputType)));
            type.Methods.Add(method);
        }
    }
}
