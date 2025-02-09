using QuickCode.AST.Expressions;
using QuickCode.AST.Symbols;

namespace QuickCode.AST.Statements;

public record class ForEachStatementAST(IdentifierAST Target, ExpressionAST List, BlockControlAST Block, IdentifierAST? Label = null) : StatementAST
{
    /// <summary>
    /// If true, foreach loop writes to the local of outter scope.
    /// If false, foreach loop writes to the local of inner scope.
    /// This value is only valid after this statement is type checked.
    /// </summary>
    public bool UseOutterTarget { get; internal set; } = false;
    public ForEachStatementAST(IdentifierAST Target, ExpressionAST List, ListAST<StatementAST> Block, IdentifierAST? Label = null)
        : this(Target, List, new BlockControlAST(Block), Label) { }
}