using QuickCode.AST.Expressions;
using QuickCode.AST.Symbols;

namespace QuickCode.AST.Statements;

public record class WhileStatementAST(ExpressionAST Condition, BlockControlAST Block, bool IsDoWhileStmt, IdentifierAST? Label = null) : StatementAST
{
    public WhileStatementAST(ExpressionAST Condition, ListAST<StatementAST> Block, bool IsDoWhileStmt, IdentifierAST? Label = null)
        : this(Condition, new BlockControlAST(Block), IsDoWhileStmt, Label) { }
}
