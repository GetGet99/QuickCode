using QuickCode.AST.Expressions;
using QuickCode.AST.FileProgram;

namespace QuickCode.AST.Classes;

/// <summary>
/// 
/// </summary>
/// <param name="DeclType">if the variable is declared to be implicit or with "var", DeclType is null</param>
/// <param name="Name"></param>
/// <param name="Expression"></param>
public record class FieldDeclStatementAST(TypeAST? DeclType, IdentifierAST Name, ExpressionAST Expression) : AST, IClassDeclarable;
