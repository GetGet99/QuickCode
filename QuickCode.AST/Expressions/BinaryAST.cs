namespace QuickCode.AST.Expressions;

public record class BinaryAST(ExpressionAST Left, ExpressionAST Right, BinaryOperators Operator) : ExpressionAST
{
    // temporary constructor while WITHPARAM is not working properly
    public BinaryAST(ExpressionAST Left, ExpressionAST Right, int Operator) : this(Left, Right, (BinaryOperators)Operator)
    {

    }
}
