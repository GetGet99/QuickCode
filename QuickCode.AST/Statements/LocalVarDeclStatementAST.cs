using QuickCode.AST.Expressions;

namespace QuickCode.AST.Statements;

/// <summary>
/// 
/// </summary>
/// <param name="DeclType">if the variable is declared to be implicit or with "var", DeclType is null</param>
/// <param name="Name"></param>
/// <param name="Expression"></param>
public record class LocalVarDeclStatementAST(TypeAST? DeclType, IdentifierAST Name, ExpressionAST Expression) : StatementAST;