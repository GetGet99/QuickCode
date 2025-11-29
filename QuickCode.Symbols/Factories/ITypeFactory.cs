namespace QuickCode.Symbols.Factories;

public interface ITypeFactory
{
    INativeTypeSymbol Boolean { get; }
    INativeTypeSymbol String { get; }
    INativeTypeSymbol Int32 { get; }
    INativeTypeSymbol Object { get; }
    INativeTypeSymbol Void { get; }
    INativeTypeSymbol Array(INativeTypeSymbol ofType);
    INativeTypeSymbol List(INativeTypeSymbol ofType);
    INativeTypeSymbol CreateGeneric(INativeTypeSymbol type, INativeTypeSymbol[] typeParameters);
    /// <summary>
    /// Declares the type into the factory.
    /// </summary>
    INativeTypeSymbol Declare(string name);
    /// <summary>
    /// Fills in the type information
    /// </summary>
    void Fill(INativeTypeSymbol nativeType, IUserTypeSymbol userType, ITypeSymbolMapper mapper);
}
public interface ITypeSymbolMapper
{
    INativeTypeSymbol this[IUserTypeSymbol userType] { get; }
}