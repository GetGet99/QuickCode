using QuickCode.Symbols;

namespace QuickCode.AST.TopLevels;

public record class TopLevelQuickCodeProgramAST(ListAST<ITopLevelDeclarable> TopLevelProgramComponentASTs) : QuickCodeAST
{
    public SymbolTable Symbols { get; internal set; } = null!;
}