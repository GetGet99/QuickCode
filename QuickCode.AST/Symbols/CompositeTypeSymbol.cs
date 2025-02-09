namespace QuickCode.AST.Symbols;

public record class CompositeTypeSymbol(SimpleTypeSymbol Base, TypeSymbol[] TypeArguments) : TypeSymbol(
    $"{Base}[{string.Join(", ", (object[])TypeArguments)}]"
)
{

}
