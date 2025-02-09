using QuickCode.AST.Expressions;

namespace QuickCode.AST;

public abstract record class TypeAST : AST;
public record class TypeIdentifierAST(IdentifierAST Name) : TypeAST;
public record class CompositeTypeAST(IdentifierAST Name, ListAST<TypeAST> TypeArguments) : TypeAST;
