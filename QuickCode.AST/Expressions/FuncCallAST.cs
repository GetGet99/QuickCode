using QuickCode.AST.Expressions.Values;
using QuickCode.Symbols;

namespace QuickCode.AST.Expressions;

public record class FuncCallAST(IdentifierAST FunctionName, ListAST<ExpressionAST> Arguments) : ExpressionAST
{
    public SingleFuncSymbol? ResolvedOverload { get; internal set; } = null;
}
public record class MethodCallAST(MemberExpressionAST FunctionName, ListAST<ExpressionAST> Arguments) : ExpressionAST;
