using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

public abstract record class ConditionalControlFlowStatementAST(ExpressionAST? Condition) : StatementAST;
