using QuickCode.AST.Expressions;

namespace QuickCode.AST;

public record class ParameterAST(IdentifierAST Name, TypeAST Type) : AST;
