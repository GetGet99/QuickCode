using QuickCode.Symbols.Functions;
using QuickCode.Symbols.Operators;

namespace QuickCode.AST.Expressions;

public record class BinaryAST(ExpressionAST Left, ExpressionAST Right, BinaryOperators Operator) : ExpressionAST, IOverloadable
{
    // temporary constructor while WITHPARAM is not working properly
    public BinaryAST(ExpressionAST Left, ExpressionAST Right, int Operator) : this(Left, Right, (BinaryOperators)Operator)
    {

    }
    public string Name => Operator.ToString();
    public IBinaryOperatorFuncSymbol ResolvedOverload { get; internal set; } = null!;
}
