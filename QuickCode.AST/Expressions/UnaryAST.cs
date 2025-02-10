namespace QuickCode.AST.Expressions;

public record class UnaryAST(ExpressionAST Expression, UnaryOperators Operator) : ExpressionAST
{
    // temporary constructor while WITHPARAM is not working properly
    public UnaryAST(ExpressionAST Expression, int Operator) : this(Expression, (UnaryOperators)Operator)
    {

    }
}

public record class UnaryWriteAST(IdentifierAST Target, UnaryWriteOperators Operator) : ExpressionAST
{
    // temporary constructor while WITHPARAM is not working properly
    public UnaryWriteAST(IdentifierAST Target, int Operator) : this(Target, (UnaryWriteOperators)Operator)
    {

    }
}