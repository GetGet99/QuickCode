using QuickCode.AST.Expressions.Values;

namespace QuickCode.AST.Expressions;

public record class FuncCallAST(IdentifierAST FunctionName, ListAST<ExpressionAST> Arguments) : ExpressionAST;
public record class MethodCallAST(MemberExpressionAST FunctionName, ListAST<ExpressionAST> Arguments) : ExpressionAST;
