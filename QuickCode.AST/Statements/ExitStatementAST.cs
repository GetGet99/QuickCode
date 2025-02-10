using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

public record class ExitStatementAST(IdentifierAST? Label, ExpressionAST? Condition) : ConditionalControlFlowStatementAST(Condition);
public record class ReturnStatementAST(ExpressionAST? ReturnValue, ExpressionAST? Condition) : ConditionalControlFlowStatementAST(Condition);
