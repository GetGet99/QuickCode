using QuickCode.Symbols.Operators;

namespace QuickCode.MSIL.Symbols;

static class MSILOperatorMap
{
    public static bool TryToUnaryOperator(string name, out UnaryOperators op)
    {
        op = name switch
        {
            "op_LogicalNot" => UnaryOperators.Not,
            "op_UnaryPlus" => UnaryOperators.Identity,
            "op_UnaryNegation" => UnaryOperators.Negate,
            _ => (UnaryOperators)(-1)
        };
        return op != (UnaryOperators)(-1);
    }
    public static string ToName(UnaryOperators unary)
    {
        return unary switch
        {
            UnaryOperators.Not => "op_LogicalNot",
            UnaryOperators.Identity => "op_UnaryPlus",
            UnaryOperators.Negate => "op_UnaryNegation",
            _ => throw new ArgumentException("Unknown operator", nameof(unary))
        };
    }
    public static bool TryToUnaryWriteOpeartorBefore(string name, out UnaryWriteOperators op)
    {
        op = name switch
        {
            "op_Increment" => UnaryWriteOperators.IncrementBefore,
            "op_Decrement" => UnaryWriteOperators.DecrementBefore,
            _ => (UnaryWriteOperators)(-1),
        };
        return op != (UnaryWriteOperators)(-1);
    }
    public static bool TryToUnaryWriteOpeartorAfter(string name, out UnaryWriteOperators op)
    {
        op = name switch
        {
            "op_Increment" => UnaryWriteOperators.IncrementAfter,
            "op_Decrement" => UnaryWriteOperators.DecrementAfter,
            _ => (UnaryWriteOperators)(-1),
        };
        return op != (UnaryWriteOperators)(-1);
    }
    public static string ToName(UnaryWriteOperators unaryWrite)
    {
        return unaryWrite switch
        {
            UnaryWriteOperators.IncrementBefore or UnaryWriteOperators.IncrementAfter => "op_Increment",
            UnaryWriteOperators.DecrementBefore or UnaryWriteOperators.DecrementAfter => "op_Decrement",
            _ => throw new ArgumentException("Unknown operator", nameof(unaryWrite))
        };
    }
    public static string? ToName(BinaryOperators binary)
    {
        return binary switch
        {
            BinaryOperators.Add => "op_Addition",
            BinaryOperators.Subtract => "op_Subtraction",
            BinaryOperators.Multiply => "op_Multiply",
            BinaryOperators.Divide => "op_Division",
            BinaryOperators.Modulo => "op_Modulus",
            BinaryOperators.Equal => "op_Equality",
            BinaryOperators.NotEqual => "op_Inequality",
            BinaryOperators.MoreThan => "op_GreaterThan",
            BinaryOperators.MoreThanOrEqual => "op_GreaterThanOrEqual",
            BinaryOperators.LessThan => "op_LessThan",
            BinaryOperators.LessThanOrEqual => "op_LessThanOrEqual",
            BinaryOperators.Range => null,
            BinaryOperators.And => null,
            BinaryOperators.Or => null,
            _ => throw new ArgumentException("Unknown operator", nameof(binary))
        };
    }
    public static bool TryToBianryOperator(string name, out BinaryOperators op)
    {
        op = name switch
        {
            "op_Addition" => BinaryOperators.Add,
            "op_Subtraction" => BinaryOperators.Subtract,
            "op_Multiply" => BinaryOperators.Multiply,
            "op_Division" => BinaryOperators.Divide,
            "op_Modulus" => BinaryOperators.Modulo,
            "op_Equality" => BinaryOperators.Equal,
            "op_Inequality" => BinaryOperators.NotEqual,
            "op_GreaterThan" => BinaryOperators.MoreThan,
            "op_GreaterThanOrEqual" => BinaryOperators.MoreThanOrEqual,
            "op_LessThan" => BinaryOperators.LessThan,
            "op_LessThanOrEqual" => BinaryOperators.LessThanOrEqual,
            _ => (BinaryOperators)(-1)
        };
        return op != (BinaryOperators)(-1);
    }
}