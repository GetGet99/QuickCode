using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

public record class LabelStatementAST(IdentifierAST Label) : StatementAST;