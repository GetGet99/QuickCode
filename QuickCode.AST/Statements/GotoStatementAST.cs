using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

public record class GotoStatementAST(IdentifierAST Label, ExpressionAST? Condition) : StatementAST;
