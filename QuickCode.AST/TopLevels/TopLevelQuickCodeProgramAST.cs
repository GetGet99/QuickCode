using QuickCode.Symbols.SymbolTables;

namespace QuickCode.AST.TopLevels;

public record class TopLevelQuickCodeProgramAST(ListAST<ITopLevelDeclarable> TopLevelProgramComponentASTs) : QuickCodeAST
{
    public IScopeSymbolTable Symbols { get; internal set; } = null!;
}