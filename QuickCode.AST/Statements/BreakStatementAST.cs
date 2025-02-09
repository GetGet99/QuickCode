using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

public record class BreakStatementAST(IdentifierAST? Label, ExpressionAST? Condition) : ConditionalControlFlowStatementAST(Condition);
