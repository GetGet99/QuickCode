using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

public record class ExprStatementAST(ExpressionAST Expression) : StatementAST;
