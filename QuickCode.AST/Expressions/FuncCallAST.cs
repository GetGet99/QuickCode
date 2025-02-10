using QuickCode.AST.Expressions.Values;

namespace QuickCode.AST.Expressions;

public record class FuncCallAST(IdentifierAST FunctionName, ListAST<ExpressionAST> Arguments) : ExpressionAST;
