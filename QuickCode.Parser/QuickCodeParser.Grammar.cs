using Get.Parser;
using Terminal = QuickCode.QuickCodeLexer.Tokens;
using NonTerminal = QuickCode.QuickCodeParser.NonTerminal;
using static QuickCode.QuickCodeParser.NonTerminal;
using QuickCode.AST;
using QuickCode.AST.TopLevels;
using QuickCode.AST.Expressions;
using QuickCode.AST.Statements;
using QuickCode.AST.Expressions.Values;
using Get.PLShared;
using QuickCode.AST.FileProgram;
using QuickCode.AST.Classes;
namespace QuickCode;
[Parser(Program, UseGetLexerTypeInformation = true)]
[Precedence(
    // for now
    Terminal.Increment, Terminal.Decrement, Associativity.Left,
    //Terminal.PostfixIncDecPrecedence, Associativity.Left,
    //Terminal.PrefixIncDecPrecedence, Associativity.Right,
    Terminal.Range, Associativity.Left,
    Terminal.Multiply, Terminal.Divide, Terminal.Modulo, Associativity.Left,
    Terminal.Plus, Terminal.Minus, Associativity.Left,
    Terminal.LessThan, Terminal.LessThanOrEqual, Terminal.MoreThan, Terminal.MoreThanOrEqual, Associativity.Left,
    Terminal.Equal, Terminal.NotEqual, Associativity.Left,
    Terminal.And, Terminal.Or, Associativity.Left,
    Terminal.Assign, Associativity.Right,
    Terminal.Comma, Associativity.Left
)]
public partial class QuickCodeParser : ParserBase<Terminal, NonTerminal, QuickCodeAST>
{
    public enum NonTerminal
    {
        [Type<QuickCodeAST>]
        [Rule(Terminal.CONTROLTOPLEVELSTATEMENTFILE, TopLevelProgram, AS, VALUE, IDENTITY)]
        //[Rule(Terminal.CONTROLNORMALFILE, FileProgram, AS, VALUE, IDENTITY)]
        Program,
        
        
        
        
        [Type<TopLevelQuickCodeProgramAST>]
        [Rule(TopLevelProgramComponents, AS, nameof(TopLevelQuickCodeProgramAST.TopLevelProgramComponentASTs), typeof(TopLevelQuickCodeProgramAST))]
        TopLevelProgram,
        [Type<ListAST<ITopLevelDeclarable>>]
        [Rule(nameof(EmptyTopLevelDeclarableList))]
        [Rule(TopLevelProgramComponents, AS, "list", TopLevelProgramComponent, AS, "value", nameof(Append))]
        [Rule(TopLevelProgramComponents, AS, VALUE, Terminal.Nop, Terminal.Newline, IDENTITY)]
        TopLevelProgramComponents,
        [Type<ITopLevelDeclarable>]
        [Rule(Statement, AS, VALUE, IDENTITY)]
        [Rule(FunctionDefinition, AS, VALUE, IDENTITY)]
        TopLevelProgramComponent,


        //[Type<QuickCodeFileProgramAST>]
        //[Rule(NamespaceList, AS, nameof(QuickCodeFileProgramAST.Namespaces), typeof(QuickCodeFileProgramAST))]
        //FileProgram,
        //[Type<ListAST<QuickCodeNamespaceAST>>]
        //NamespaceList,
        //[Type<QuickCodeNamespaceAST>]
        //[Rule(NamespaceList, AS, nameof(QuickCodeNamespaceAST.Classes), typeof(QuickCodeNamespaceAST))]
        ////[Rule(
        ////    Terminal.Namespace,
        ////    //Terminal.Identifier, AS, nameof(QuickCodeClassAST.Name),
        ////    Terminal.Colon, Terminal.Newline,
        ////    Terminal.Indent,
        ////        //ClassDeclarables, AS, nameof(QuickCodeClassAST.Declarables),
        ////    Terminal.Dedent,
        ////    typeof(QuickCodeClassAST)
        ////)]
        //Namespace,
        //[Type<ListAST<QuickCodeClassAST>>]
        
        //NamespaceDeclarableList,
        //[Type<QuickCodeClassAST>]
        //[Rule(
        //    Terminal.Class,
        //    Terminal.Identifier, AS, nameof(QuickCodeClassAST.Name),
        //    Terminal.Colon, Terminal.Newline,
        //    Terminal.Indent,
        //        ClassDeclarables, AS, nameof(QuickCodeClassAST.Declarables),
        //    Terminal.Dedent,
        //    typeof(QuickCodeClassAST)
        //)]
        //Class,
        //[Type<ListAST<IClassDeclarable>>]
        //ClassDeclarables,
        //[Type<IClassDeclarable>]
        //[Rule(FieldDeclaration, AS, "value", IDENTITY)]
        //[Rule(FunctionDefinition, AS, "value", IDENTITY)]
        //ClassDeclarable,
        //[Type<FieldDeclStatementAST>]
        //[Rule(Terminal.Identifier, AS, nameof(FieldDeclStatementAST.Name),
        //    Terminal.Colon,
        //    Type, AS, nameof(FieldDeclStatementAST.DeclType),
        //    Terminal.Assign,
        //    Expression, AS, nameof(FieldDeclStatementAST.Expression),
        //    Terminal.Newline,
        //    typeof(FieldDeclStatementAST))]
        //FieldDeclaration,


        /// <summary>
        /// Represents list of 1+ statements. This list can empty if nop is used. Otherwise this list is not empty.
        /// </summary>
        [Type<ListAST<StatementAST>>]
        [Rule(Statement, AS, "value", nameof(Single))]
        [Rule(Terminal.Nop, Terminal.Newline, nameof(EmptyStatementList))]
        [Rule(BlockStatements, AS, "list", Statement, AS, "value", nameof(Append))]
        [Rule(BlockStatements, AS, VALUE, Terminal.Nop, Terminal.Newline, IDENTITY)]
        BlockStatements,
        /// <summary>
        /// Represents list of 1+ statements. This list can empty if nop is used. Otherwise this list is not empty.
        /// May be (without block word)
        /// <code>
        /// block:
        ///     nop
        ///     
        /// block:
        ///     stmt
        ///     stmt
        ///     ...
        ///     stmt
        /// block: nop
        /// block: simpleStmt
        /// // future:
        /// // block: simpleStmt; simpleStmt; ... simpleStmt
        /// </code>
        /// </summary>
        [Type<ListAST<StatementAST>>]
        [Rule(
            Terminal.Colon, Terminal.Newline,
            Terminal.Indent,
                BlockStatements, AS, VALUE,
            Terminal.Dedent,
            IDENTITY)]
        [Rule(
            Terminal.Colon, Terminal.Nop, Terminal.Newline,
            nameof(EmptyStatementList))]
        [Rule(
            Terminal.Colon, SimpleStatement, AS, "value",
            nameof(Single))]
        BlockStatementsWithColonIndentDedent,



        /// <summary>
        /// A statement that is allowed as part of inline block call
        /// </summary>
        [Type<StatementAST>]
        [Rule(Expression, AS, nameof(ExprStatementAST.Expression), Terminal.Newline, typeof(ExprStatementAST))]
        [Rule(
            Terminal.Goto, Terminal.DollarSign, Terminal.Identifier, AS, nameof(GotoStatementAST.Label), Terminal.Newline,
            WITHPARAM, nameof(GotoStatementAST.Condition), null, typeof(GotoStatementAST))]
        [Rule(
            Terminal.Goto, Terminal.DollarSign, Terminal.Identifier, AS, nameof(GotoStatementAST.Label),
            Terminal.If, Expression, AS, nameof(GotoStatementAST.Condition), Terminal.Newline,
            typeof(GotoStatementAST))]
        SimpleStatement,
        /// <summary>
        /// A statement
        /// </summary>
        [Type<StatementAST>]
        [Rule(Terminal.Identifier, AS, nameof(LocalVarDeclStatementAST.Name),
            Terminal.Colon,
            Type, AS, nameof(LocalVarDeclStatementAST.DeclType),
            Terminal.Assign,
            Expression, AS, nameof(LocalVarDeclStatementAST.Expression),
            Terminal.Newline,
            typeof(LocalVarDeclStatementAST))]
        [Rule(Terminal.Var,
            Terminal.Identifier, AS, nameof(LocalVarDeclStatementAST.Name),
            Terminal.Assign,
            Expression, AS, nameof(LocalVarDeclStatementAST.Expression),
            Terminal.Newline,
            WITHPARAM, nameof(LocalVarDeclStatementAST.DeclType), null,
            typeof(LocalVarDeclStatementAST))]
        [Rule(
            Terminal.Identifier, AS, nameof(LocalVarDeclStatementAST.Name),
            Terminal.Declare,
            Expression, AS, nameof(LocalVarDeclStatementAST.Expression),
            Terminal.Newline,
            WITHPARAM, nameof(LocalVarDeclStatementAST.DeclType), null,
            typeof(LocalVarDeclStatementAST))]
        [Rule(
            Terminal.While, OptionalLabel, AS, nameof(WhileStatementAST.Label),
            Expression, AS, nameof(WhileStatementAST.Condition),
            BlockStatementsWithColonIndentDedent, AS, nameof(WhileStatementAST.Block),
            WITHPARAM, nameof(WhileStatementAST.IsDoWhileStmt), false,
            typeof(WhileStatementAST))]
        [Rule(
            Terminal.Do, OptionalLabel, AS, nameof(WhileStatementAST.Label),
            BlockStatementsWithColonIndentDedent, AS, nameof(WhileStatementAST.Block),
            Terminal.While, Expression, AS, nameof(WhileStatementAST.Condition),
            Terminal.Newline, // new line after while expr
            WITHPARAM, nameof(WhileStatementAST.IsDoWhileStmt), true,
            typeof(WhileStatementAST))]
        [Rule(
            Terminal.If, OptionalLabel, AS, nameof(IfStatementAST.Label),
            Expression, AS, nameof(IfStatementAST.Condition),
            BlockStatementsWithColonIndentDedent, AS, nameof(IfStatementAST.TrueBlock),
            ElsePart, AS, nameof(IfStatementAST.FalseBlock),
            typeof(IfStatementAST))]
        [Rule(
            Terminal.For, OptionalLabel, AS, nameof(ForEachStatementAST.Label),
            Terminal.Identifier, AS, nameof(ForEachStatementAST.Target),
            Terminal.In,
            Expression, AS, nameof(ForEachStatementAST.List),
            BlockStatementsWithColonIndentDedent, AS, nameof(ForEachStatementAST.Block),
            typeof(ForEachStatementAST))]
        [Rule(SimpleStatement, AS, VALUE, IDENTITY)]
        // gotos and labels
        [Rule(
            Terminal.Dedent,
            Terminal.DollarSign, Terminal.Identifier, AS, nameof(LabelStatementAST.Label), Terminal.Colon, Terminal.Newline,
            Terminal.Indent, typeof(LabelStatementAST))]
        [Rule(
            Terminal.DefLabel, Terminal.DollarSign, Terminal.Identifier, AS, nameof(LabelStatementAST.Label), Terminal.Newline,
            typeof(LabelStatementAST))]
        [Rule(
            Terminal.Break,
            OptionalLabel, AS, nameof(BreakStatementAST.Label),
            OptionalIfTag, AS, nameof(BreakStatementAST.Condition), Terminal.Newline,
            typeof(BreakStatementAST))]
        [Rule(
            Terminal.Continue,
            OptionalLabel, AS, nameof(ContinueStatementAST.Label),
            OptionalIfTag, AS, nameof(ContinueStatementAST.Condition), Terminal.Newline,
            typeof(ContinueStatementAST))]
        [Rule(
            Terminal.Exit,
            OptionalLabel, AS, nameof(ExitStatementAST.Label),
            OptionalIfTag, AS, nameof(ExitStatementAST.Condition), Terminal.Newline,
            typeof(ExitStatementAST))]
        [Rule(
            Terminal.Return,
            OptionalExpression, AS, nameof(ReturnStatementAST.ReturnValue),
            OptionalIfTag, AS, nameof(BreakStatementAST.Condition), Terminal.Newline,
            typeof(ReturnStatementAST))]
        Statement,
        [Type<ListAST<StatementAST>>]
        [Rule(
            Terminal.Else, Terminal.If, OptionalLabel, AS, "label",
            Expression, AS, "cond",
            BlockStatementsWithColonIndentDedent, AS, "trueStmts",
            ElsePart, AS, "elsePart",
            nameof(CreateElseIfElsePart))]
        [Rule(
            Terminal.Else,
            BlockStatementsWithColonIndentDedent, AS, VALUE,
            IDENTITY)]
        [Rule(nameof(EmptyStatementList))]
        ElsePart,




        // EXPRESSIONS

        // Binary Expressions
        [Type<ExpressionAST>]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.Plus,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Add,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.Minus,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Subtract,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.Multiply,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Multiply,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.Divide,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Divide,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.Modulo,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Modulo,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.LessThan,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.LessThan,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.LessThanOrEqual,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.LessThanOrEqual,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.MoreThan,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.MoreThan,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.MoreThanOrEqual,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.MoreThanOrEqual,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.Equal,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Equal,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.NotEqual,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.NotEqual,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.Range,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Range,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.And,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.And,
            typeof(BinaryAST))]
        [Rule(Expression, AS, nameof(BinaryAST.Left),
            Terminal.Or,
            Expression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Or,
            typeof(BinaryAST))]


        // Unary Expressions
        [Rule(
            Terminal.Plus,
            Expression, AS, nameof(UnaryAST.Expression),
            WITHPARAM, nameof(UnaryAST.Operator), UnaryOperators.Identity,
            typeof(UnaryAST))]
        [Rule(
            Terminal.Minus,
            Expression, AS, nameof(UnaryAST.Expression),
            WITHPARAM, nameof(UnaryAST.Operator), UnaryOperators.Negate,
            typeof(UnaryAST))]
        [Rule(
            Terminal.Increment,
            Terminal.Identifier, AS, nameof(UnaryWriteAST.Target),
            WITHPARAM, nameof(UnaryWriteAST.Operator), UnaryWriteOperators.IncrementBefore,
            typeof(UnaryWriteAST))]
        [Rule(
            Terminal.Identifier, AS, nameof(UnaryWriteAST.Target),
            Terminal.Increment,
            WITHPARAM, nameof(UnaryWriteAST.Operator), UnaryWriteOperators.IncrementAfter,
            typeof(UnaryWriteAST))]
        [Rule(
            Terminal.Decrement,
            Terminal.Identifier, AS, nameof(UnaryWriteAST.Target),
            WITHPARAM, nameof(UnaryWriteAST.Operator), UnaryWriteOperators.DecrementBefore,
            typeof(UnaryWriteAST))]
        [Rule(
            Terminal.Identifier, AS, nameof(UnaryWriteAST.Target),
            Terminal.Decrement,
            WITHPARAM, nameof(UnaryWriteAST.Operator), UnaryWriteOperators.DecrementAfter,
            typeof(UnaryWriteAST))]

        // Assign
        [Rule(Terminal.Identifier, AS, nameof(AssignAST.Left),
            Terminal.Assign,
            Expression, AS, nameof(AssignAST.Right),
            typeof(AssignAST))]
        
        // Constants and Identifier
        [Rule(Terminal.Integer, AS, nameof(Int32ValueAST.Value), typeof(Int32ValueAST))]
        [Rule(Terminal.String, AS, nameof(StringValueAST.Value), typeof(StringValueAST))]
        [Rule(Terminal.Boolean, AS, nameof(BooleanValueAST.Value), typeof(BooleanValueAST))]
        [Rule(Terminal.Identifier, AS, VALUE, IDENTITY)]

        // Function Call
        [Rule(Terminal.Identifier, AS, nameof(FuncCallAST.FunctionName),
            Terminal.OpenBracket,
            CommaSeparatedExpression, AS, nameof(FuncCallAST.Arguments),
            Terminal.CloseBracket,
            typeof(FuncCallAST))]
        // list and array creation
        [Rule(Terminal.List, Terminal.OpenSquareBracket,
            CommaSeparatedExpression, AS, nameof(ListDeclarationAST.Elements),
            Terminal.CloseSquareBracket,
            typeof(ListDeclarationAST))]
        [Rule(Terminal.Array, Terminal.OpenSquareBracket,
            CommaSeparatedExpression, AS, nameof(ArrayDeclarationWithValuesAST.Elements),
            Terminal.CloseSquareBracket,
            typeof(ArrayDeclarationWithValuesAST))]
        Expression,

        [Type<FunctionAST>]


        // Function Definitions
        // func fname: ...
        [Rule(Terminal.Func, Terminal.Identifier, AS, nameof(FunctionAST.Name),
            BlockStatementsWithColonIndentDedent, AS, nameof(FunctionAST.Statements),
            typeof(FunctionAST))]
        // func fname -> retType: ...
        [Rule(Terminal.Func, Terminal.Identifier, AS, nameof(FunctionAST.Name),
            Terminal.Arrow, Type, AS, nameof(FunctionAST.ReturnType),
            BlockStatementsWithColonIndentDedent, AS, nameof(FunctionAST.Statements),
            typeof(FunctionAST))]
        // func fname(parameters): ...
        [Rule(Terminal.Func, Terminal.Identifier, AS, nameof(FunctionAST.Name),
            Terminal.OpenBracket, ParametersList, AS, nameof(FunctionAST.Parameters), Terminal.CloseBracket,
            BlockStatementsWithColonIndentDedent, AS, nameof(FunctionAST.Statements),
            typeof(FunctionAST))]
        // func fname(parameters) -> retType: ...
        [Rule(Terminal.Func, Terminal.Identifier, AS, nameof(FunctionAST.Name),
            Terminal.OpenBracket, ParametersList, AS, nameof(FunctionAST.Parameters), Terminal.CloseBracket,
            Terminal.Arrow, Type, AS, nameof(FunctionAST.ReturnType),
            BlockStatementsWithColonIndentDedent, AS, nameof(FunctionAST.Statements),
            typeof(FunctionAST))]
        FunctionDefinition,


        // Optional Label
        [Type<IdentifierAST>]
        [Rule(nameof(NullIdentifier))]
        [Rule(Terminal.DollarSign, Terminal.Identifier, AS, VALUE, IDENTITY)]
        OptionalLabel,


        // Optional If Tag
        [Type<ExpressionAST>]
        [Rule(nameof(NullExpression))]
        [Rule(Terminal.If, Expression, AS, VALUE, IDENTITY)]
        OptionalIfTag,

        // Optional Expression
        [Type<ExpressionAST>]
        [Rule(nameof(NullExpression))]
        [Rule(Expression, AS, VALUE, IDENTITY)]
        OptionalExpression,

        // Arguments List
        [Type<ListAST<ExpressionAST>>]
        [Rule(nameof(EmptyExpressionList))]
        [Rule(NonEmptyCommaSepratedExpression, AS, VALUE, IDENTITY)]
        CommaSeparatedExpression,
        [Type<ListAST<ExpressionAST>>]
        [Rule(Expression, AS, "value", nameof(Single))]
        [Rule(NonEmptyCommaSepratedExpression, AS, "list", Terminal.Comma, Expression, AS, "value", nameof(Append))]
        NonEmptyCommaSepratedExpression,

        // Parameters List
        [Type<ListAST<ParameterAST>>]
        [Rule(nameof(EmptyParameterList))]
        [Rule(UntypedParametersList, AS, "untypedParams", Terminal.Colon, Type, AS, "type", nameof(UntypedToTypedParams))]
        [Rule(ParametersList, AS, "list1", Terminal.Comma, ParametersList, AS, "list2", nameof(Combine))]
        ParametersList,
        // Untyped Parameters List
        [Type<ListAST<IdentifierAST>>]
        [Rule(Terminal.Identifier, AS, "value", nameof(Single))]
        [Rule(UntypedParametersList, AS, "list", Terminal.Comma, Terminal.Identifier, AS, "value", nameof(Append))]
        UntypedParametersList,

        // Types
        [Type<TypeAST>]
        [Rule(IdentifierOrReserved, AS, nameof(TypeIdentifierAST.Name), typeof(TypeIdentifierAST))]
        [Rule(
            IdentifierOrReserved, AS, nameof(CompositeTypeAST.Name),
            Terminal.OpenSquareBracket,
            CommaSeparatedType, AS, nameof(CompositeTypeAST.TypeArguments),
            Terminal.CloseSquareBracket,
            typeof(CompositeTypeAST)
        )]
        Type,
        [Type<ListAST<TypeAST>>]
        [Rule(Type, AS, "value", nameof(Single))]
        [Rule(CommaSeparatedType, AS, "list", Terminal.Comma, Type, AS, "value", nameof(Append))]
        CommaSeparatedType,
        [Type<IdentifierAST>]
        [Rule(Terminal.List, AS, VALUE, IDENTITY)]
        [Rule(Terminal.Array, AS, VALUE, IDENTITY)]
        [Rule(Terminal.Identifier, AS, VALUE, IDENTITY)]
        IdentifierOrReserved
    }
    static IdentifierAST? NullIdentifier()
        => null;
    static ExpressionAST? NullExpression()
        => null;
    static ListAST<StatementAST> CreateElseIfElsePart(ExpressionAST cond, IdentifierAST? label, ListAST<StatementAST> trueStmts, ListAST<StatementAST> elsePart)
        => [
            new IfStatementAST(cond, trueStmts, elsePart, label)
        ];
    static T Identity<T>(T value) => value;
    static ListAST<ITopLevelDeclarable> EmptyTopLevelDeclarableList() => [];
    static ListAST<StatementAST> EmptyStatementList() => [];
    static ListAST<ExpressionAST> EmptyExpressionList() => [];
    static ListAST<ParameterAST> EmptyParameterList() => [];
    static ListAST<ParameterAST> UntypedToTypedParams(ListAST<IdentifierAST> untypedParams, TypeAST type)
    {
        ListAST<ParameterAST> typedParams = new();
        foreach (var id in untypedParams)
            typedParams.Add(new(id, type));
        return typedParams;
    }
    static ListAST<T> Append<T>(ListAST<T> list, T value)
    {
        list.Add(value);
        return list;
    }
    static ListAST<T> Combine<T>(ListAST<T> list1, ListAST<T> list2)
    {
        foreach (var val in list2)
            list1.Add(val);
        return list1;
    }
    static ListAST<T> Single<T>(T value) => [value];
    static ListAST<T> AppendOrNop<T>(ListAST<T> list, T? value)
    {
        if (value is not null)
            list.Add(value);
        return list;
    }
    static ListAST<T> SingleOrEmpty<T>(T? value) => value is null ? [] : [value];
    public QuickCodeAST Parse(IEnumerable<IToken<Terminal>> inputTerminals)
    {
        IEnumerable<ITerminalValue> TerminalValues()
        {
            foreach (var inputTerminal in inputTerminals)
            {
                // TO DO: Terminal with Value general implementation
                if (inputTerminal.TokenType is Terminal.Identifier)
                    yield return CreateValue(inputTerminal.TokenType, ((IToken<Terminal, IdentifierAST>)inputTerminal).Data);
                else if (inputTerminal.TokenType is Terminal.Integer)
                    yield return CreateValue(inputTerminal.TokenType, ((IToken<Terminal, int>)inputTerminal).Data);
                else if (inputTerminal.TokenType is Terminal.Boolean)
                    yield return CreateValue(inputTerminal.TokenType, ((IToken<Terminal, bool>)inputTerminal).Data);
                else if (inputTerminal.TokenType is Terminal.String)
                    yield return CreateValue(inputTerminal.TokenType, ((IToken<Terminal, string>)inputTerminal).Data);
                else
                    yield return CreateValue(inputTerminal.TokenType);
            }
        }
        return Parse(TerminalValues());
    }
}
