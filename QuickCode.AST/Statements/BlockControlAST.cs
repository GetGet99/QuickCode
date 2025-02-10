using QuickCode.Symbols;

namespace QuickCode.AST.Statements;

public record class BlockControlAST(ListAST<StatementAST> Statements) : AST

{
    /// <summary>
    /// Represents the symbol table of the inner symbols.
    /// This is only valid after the statement has been type checked.
    /// </summary>
    public SymbolTable InnerSymbols { get; internal set; } = null!;
}
