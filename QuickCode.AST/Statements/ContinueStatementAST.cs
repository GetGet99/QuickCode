using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

public record class ContinueStatementAST(IdentifierAST? Label, ExpressionAST? Condition) : ConditionalControlFlowStatementAST(Condition);
