namespace QuickCode.AST.Symbols;

public abstract record class TypeSymbol(string Name) : Symbol
{
    public static SimpleTypeSymbol Boolean { get; } = new("<built in>.Boolean");
    public static SimpleTypeSymbol String { get; } = new("<built in>.String");
    public static SimpleTypeSymbol Int32 { get; } = new("<built in>.Int32");
    public static SimpleTypeSymbol Object { get; } = new("<built in>.Object");
    public static SimpleTypeSymbol Void { get; } = new("<built in>.Void");
    /// <summary>
    /// A type only used while doing the type checking. This type may not exist at the end of the
    /// type checking state if the program is well-typed.
    /// well-typed.
    /// </summary>
    public static SimpleTypeSymbol Any { get; } = new("[Any]");
    public static SimpleTypeSymbol UntypedArray { get; } = new("<built in>.Array");
    public static SimpleTypeSymbol UntypedList { get; } = new("<built in>.List");
    public static CompositeTypeSymbol Array(TypeSymbol type) => new(UntypedArray, [type]);
    public static CompositeTypeSymbol List(TypeSymbol type) => new(UntypedList, [type]);
    public SymbolTable Children { get; } = new();
}
