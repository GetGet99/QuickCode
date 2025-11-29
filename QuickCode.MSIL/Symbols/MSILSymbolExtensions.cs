using Mono.Cecil;
using QuickCode.Symbols;
using QuickCode.Symbols.Functions;

namespace QuickCode.MSIL.Symbols;

static class MSILSymbolExtensions
{
    public static IFieldSymbol Symbol(this FieldDefinition field)
        => new MSILFieldSymbol(field);
    public static INativeTypeSymbol Symbol(this TypeReference type)
        => new MSILTypeSymbol(type);
    public static IFuncSymbol SymbolFunc(this MethodDefinition type)
        => new MSILFuncSymbol(type);
    public static IBinaryOperatorFuncSymbol SymbolBinary(this MethodDefinition type)
        => new MSILBinaryFuncSymbol(type);
    public static IUnaryOperatorFuncSymbol SymbolUnary(this MethodDefinition type)
        => new MSILUnaryFuncSymbol(type);
    public static IUnaryWriteOperatorFuncSymbol SymbolUnaryWrite(this MethodDefinition type, bool before)
        => new MSILUnaryWriteFuncSymbol(type, before);
    public static TypeReference MSIL(this INativeTypeSymbol type)
        => ((MSILTypeSymbol)type).MSILType;
}
