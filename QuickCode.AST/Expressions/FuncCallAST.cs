using QuickCode.AST.Expressions.Values;
using QuickCode.Symbols;
using QuickCode.Symbols.Functions;

namespace QuickCode.AST.Expressions;

public record class FuncCallAST(IdentifierAST FunctionName, ListAST<ExpressionAST> Arguments) : ExpressionAST
{
    public IFuncSymbol? ResolvedOverload { get; internal set; } = null;
}
public record class NewObjectAST(TypeAST TypeName, ListAST<ExpressionAST> Arguments) : ExpressionAST
{
    public IFuncSymbol? ResolvedOverload { get; internal set; } = null;
}
public record class MethodCallAST(MemberExpressionAST FunctionName, ListAST<ExpressionAST> Arguments) : ExpressionAST
{
    public IFuncSymbol? ResolvedOverload { get; internal set; } = null;
    public bool IsStaticCall { get; internal set; } = false;
}
