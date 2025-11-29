using QuickCode.Symbols.Functions;
using QuickCode.Symbols.Operators;

namespace QuickCode.AST.Expressions;

public record class UnaryAST(ExpressionAST Expression, UnaryOperators Operator) : ExpressionAST, IOverloadable
{
    // temporary constructor while WITHPARAM is not working properly
    public UnaryAST(ExpressionAST Expression, int Operator) : this(Expression, (UnaryOperators)Operator)
    {

    }

    public string Name => Operator.ToString();
    public IUnaryOperatorBaseFuncSymbol ResolvedOverload { get; internal set; } = null!;
}

public record class UnaryWriteAST(IdentifierAST Target, UnaryWriteOperators Operator) : ExpressionAST, IOverloadable
{
    // temporary constructor while WITHPARAM is not working properly
    public UnaryWriteAST(IdentifierAST Target, int Operator) : this(Target, (UnaryWriteOperators)Operator)
    {

    }
    public string Name => Operator.ToString();
    public IUnaryOperatorBaseFuncSymbol ResolvedOverload { get; internal set; } = null!;
}