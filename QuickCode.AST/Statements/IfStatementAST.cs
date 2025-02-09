using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

public record class IfStatementAST(ExpressionAST Condition, BlockControlAST TrueBlock, BlockControlAST FalseBlock, IdentifierAST? Label = null) : StatementAST
{
    public IfStatementAST(ExpressionAST Condition, ListAST<StatementAST> TrueBlock, ListAST<StatementAST> FalseBlock, IdentifierAST? Label = null)
        : this(Condition, new BlockControlAST(TrueBlock), new BlockControlAST(FalseBlock), Label) { }
}
