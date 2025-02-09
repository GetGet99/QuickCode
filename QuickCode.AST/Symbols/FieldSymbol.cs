namespace QuickCode.AST.Symbols;

public record class FieldSymbol(TypeSymbol Type, string Name) : Symbol;
