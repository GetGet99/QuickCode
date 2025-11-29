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
using QuickCode.Symbols.Operators;
namespace QuickCode;
[Parser(Program, UseGetLexerTypeInformation = true)]
[Precedence(
    // for now
    Terminal.Increment, Terminal.Decrement, Terminal.Dot, Associativity.Left,
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
        [Rule(Terminal.CONTROLNORMALFILE, FileProgram, AS, VALUE, IDENTITY)]
        Program,
        
        
        
        
        [Type<TopLevelQuickCodeProgramAST>]
        [Rule(TopLevelProgramComponents, AS, nameof(TopLevelQuickCodeProgramAST.TopLevelProgramComponentASTs), typeof(TopLevelQuickCodeProgramAST))]
        TopLevelProgram,
        [Type<ListAST<ITopLevelDeclarable>>]
        [Rule(EMPTYLIST)]
        [Rule(TopLevelProgramComponents, AS, LIST, TopLevelProgramComponent, AS, VALUE, APPENDLIST)]
        [Rule(TopLevelProgramComponents, AS, VALUE, Terminal.Nop, Terminal.Newline, IDENTITY)]
        TopLevelProgramComponents,
        [Type<ITopLevelDeclarable>]
        [Rule(Statement, AS, VALUE, IDENTITY)]
        [Rule(FunctionDefinition, AS, VALUE, IDENTITY)]
        [Rule(ClassDefinition, AS, VALUE, IDENTITY)]
        TopLevelProgramComponent,


        [Type<QuickCodeFileProgramAST>]
        [Rule(NamespaceList, AS, nameof(QuickCodeFileProgramAST.Namespaces), typeof(QuickCodeFileProgramAST))]
        FileProgram,
        [Type<ListAST<QuickCodeNamespaceAST>>]
        [Rule(Terminal.Nop, Terminal.Newline, EMPTYLIST)]
        [Rule(Namespace, AS, VALUE, SINGLELIST)]
        [Rule(NamespaceList, AS, LIST, Namespace, AS, VALUE, APPENDLIST)]
        NamespaceList,
        [Type<QuickCodeNamespaceAST>]
        [Rule(
            Terminal.Namespace,
            // list of identifiers
            DotSeparatedIdentifier, AS, nameof(QuickCodeNamespaceAST.Name),
            Terminal.Colon, Terminal.Newline,
            Terminal.Indent,
            NamespaceDeclarableList, AS, nameof(QuickCodeNamespaceAST.Types),
            Terminal.Dedent,
            typeof(QuickCodeNamespaceAST)
        )]
        Namespace,

        [Type<ListAST<QuickCodeClassAST>>]
        [Rule(Terminal.Nop, Terminal.Newline, EMPTYLIST)]
        [Rule(NamespaceDeclarable, AS, VALUE, SINGLELIST)]
        [Rule(NamespaceDeclarableList, AS, LIST, NamespaceDeclarable, AS, VALUE, APPENDLIST)]
        NamespaceDeclarableList,
        [Type<QuickCodeClassAST>]
        [Rule(ClassDefinition, AS, VALUE, IDENTITY)]
        NamespaceDeclarable,
        [Type<QuickCodeClassAST>]
        [Rule(
            Terminal.Class,
            Type, AS, nameof(QuickCodeClassAST.Name),
            Terminal.Colon, Terminal.Newline,
            Terminal.Indent,
                ClassDeclarables, AS, nameof(QuickCodeClassAST.Declarables),
            Terminal.Dedent,
            typeof(QuickCodeClassAST)
        )]
        ClassDefinition,
        [Type<ListAST<IClassDeclarable>>]
        [Rule(Terminal.Nop, Terminal.Newline, EMPTYLIST)]
        [Rule(ClassDeclarable, AS, VALUE, SINGLELIST)]
        [Rule(ClassDeclarables, AS, LIST, ClassDeclarable, AS, VALUE, APPENDLIST)]
        ClassDeclarables,
        [Type<IClassDeclarable>]
        [Rule(FieldDeclaration, AS, VALUE, IDENTITY)]
        [Rule(FunctionDefinition, AS, VALUE, IDENTITY)]
        ClassDeclarable,
        [Type<FieldDeclStatementAST>]
        [Rule(Terminal.Identifier, AS, nameof(FieldDeclStatementAST.Name),
            Terminal.Colon,
            Type, AS, nameof(FieldDeclStatementAST.DeclType),
            Terminal.Assign,
            Expression, AS, nameof(FieldDeclStatementAST.Expression),
            Terminal.Newline,
            typeof(FieldDeclStatementAST))]
        FieldDeclaration,
        /// <summary>
        /// Represents list of 1+ statements. This list can empty if nop is used. Otherwise this list is not empty.
        /// </summary>
        [Type<ListAST<StatementAST>>]
        [Rule(Statement, AS, VALUE, SINGLELIST)]
        [Rule(Terminal.Nop, Terminal.Newline, EMPTYLIST)]
        [Rule(BlockStatements, AS, LIST, Statement, AS, VALUE, APPENDLIST)]
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
            EMPTYLIST)]
        [Rule(
            Terminal.Colon, SimpleStatement, AS, VALUE, SINGLELIST)]
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
        [Rule(EMPTYLIST)]
        ElsePart,




        // EXPRESSIONS

        // Binary Expressions
        [Type<ExpressionAST>]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.Plus,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Add,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.Minus,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Subtract,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.Multiply,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Multiply,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.Divide,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Divide,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.Modulo,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Modulo,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.LessThan,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.LessThan,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.LessThanOrEqual,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.LessThanOrEqual,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.MoreThan,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.MoreThan,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.MoreThanOrEqual,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.MoreThanOrEqual,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.Equal,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Equal,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.NotEqual,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.NotEqual,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.Range,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Range,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.And,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.And,
            typeof(BinaryAST))]
        [Rule(CExpression, AS, nameof(BinaryAST.Left),
            Terminal.Or,
            CExpression, AS, nameof(BinaryAST.Right),
            WITHPARAM, nameof(BinaryAST.Operator), BinaryOperators.Or,
            typeof(BinaryAST))]


        // Unary Expressions
        [Rule(
            Terminal.Plus,
            CExpression, AS, nameof(UnaryAST.Expression),
            WITHPARAM, nameof(UnaryAST.Operator), UnaryOperators.Identity,
            typeof(UnaryAST))]
        [Rule(
            Terminal.Minus,
            CExpression, AS, nameof(UnaryAST.Expression),
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
            CExpression, AS, nameof(AssignAST.Right),
            typeof(AssignAST))]
        
        // Constants and Identifier
        [Rule(Terminal.Integer, AS, nameof(Int32ValueAST.Value), typeof(Int32ValueAST))]
        [Rule(Terminal.String, AS, nameof(StringValueAST.Value), typeof(StringValueAST))]
        [Rule(Terminal.Boolean, AS, nameof(BooleanValueAST.Value), typeof(BooleanValueAST))]
        [Rule(Terminal.Identifier, AS, VALUE, IDENTITY)]

        // New
        [Rule(Terminal.New, Type, AS, nameof(NewObjectAST.TypeName),
            Terminal.OpenBracket,
            CommaSeparatedExpression, AS, nameof(NewObjectAST.Arguments),
            Terminal.CloseBracket,
            typeof(NewObjectAST))]
        // Function Call
        [Rule(Terminal.Identifier, AS, nameof(FuncCallAST.FunctionName),
            Terminal.OpenBracket,
            CommaSeparatedExpression, AS, nameof(FuncCallAST.Arguments),
            Terminal.CloseBracket,
            typeof(FuncCallAST))]
        [Rule(MemberExpression, AS, nameof(MethodCallAST.FunctionName),
            Terminal.OpenBracket,
            CommaSeparatedExpression, AS, nameof(MethodCallAST.Arguments),
            Terminal.CloseBracket,
            typeof(MethodCallAST))]
        // list and array creation
        [Rule(Terminal.List, Terminal.OpenSquareBracket,
            CommaSeparatedExpression, AS, nameof(ListDeclarationAST.Elements),
            Terminal.CloseSquareBracket,
            typeof(ListDeclarationAST))]
        [Rule(Terminal.Array, Terminal.OpenSquareBracket,
            CommaSeparatedExpression, AS, nameof(ArrayDeclarationWithValuesAST.Elements),
            Terminal.CloseSquareBracket,
            typeof(ArrayDeclarationWithValuesAST))]
        // (expr)
        [Rule(Terminal.OpenBracket, Expression, AS, VALUE, Terminal.CloseBracket, IDENTITY)]

        // Members
        [Rule(MemberExpression, AS, VALUE, IDENTITY)]
        CExpression,
        [Type<ExpressionAST>]
        [Rule(CExpression, AS, VALUE, IDENTITY)]
        Expression,
        [Type<MemberExpressionAST>]
        [Rule(
            CExpression, AS, nameof(MemberExpressionAST.Expression),
            Terminal.Dot,
            Terminal.Identifier, AS, nameof(MemberExpressionAST.Member),
            typeof(MemberExpressionAST))]
        MemberExpression,

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
        [Rule(WITHPARAM, VALUE, null, IDENTITY)]
        [Rule(Terminal.DollarSign, Terminal.Identifier, AS, VALUE, IDENTITY)]
        OptionalLabel,


        // Optional If Tag
        [Type<ExpressionAST>]
        [Rule(WITHPARAM, VALUE, null, IDENTITY)]
        [Rule(Terminal.If, Expression, AS, VALUE, IDENTITY)]
        OptionalIfTag,

        // Optional Expression
        [Type<ExpressionAST>]
        [Rule(WITHPARAM, VALUE, null, IDENTITY)]
        [Rule(Expression, AS, VALUE, IDENTITY)]
        OptionalExpression,

        // Arguments List
        [Type<ListAST<ExpressionAST>>]
        [Rule(EMPTYLIST)]
        [Rule(NonEmptyCommaSepratedExpression, AS, VALUE, IDENTITY)]
        CommaSeparatedExpression,
        [Type<ListAST<ExpressionAST>>]
        [Rule(Expression, AS, VALUE, SINGLELIST)]
        [Rule(NonEmptyCommaSepratedExpression, AS, LIST, Terminal.Comma, Expression, AS, VALUE, APPENDLIST)]
        NonEmptyCommaSepratedExpression,

        // Parameters List
        [Type<ListAST<ParameterAST>>]
        [Rule(EMPTYLIST)]
        [Rule(UntypedParametersList, AS, "untypedParams", Terminal.Colon, Type, AS, "type", nameof(UntypedToTypedParams))]
        [Rule(ParametersList, AS, "list1", Terminal.Comma, ParametersList, AS, "list2", nameof(Combine))]
        ParametersList,
        // Untyped Parameters List
        [Type<ListAST<IdentifierAST>>]
        [Rule(Terminal.Identifier, AS, VALUE, SINGLELIST)]
        [Rule(UntypedParametersList, AS, LIST, Terminal.Comma, Terminal.Identifier, AS, VALUE, APPENDLIST)]
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
        [Rule(Type, AS, VALUE, SINGLELIST)]
        [Rule(CommaSeparatedType, AS, LIST, Terminal.Comma, Type, AS, VALUE, APPENDLIST)]
        CommaSeparatedType,
        [Type<ListAST<IdentifierAST>>]
        [Rule(Terminal.Identifier, AS, VALUE, SINGLELIST)]
        [Rule(DotSeparatedIdentifier, AS, LIST, Terminal.Dot, Terminal.Identifier, AS, VALUE, APPENDLIST)]
        DotSeparatedIdentifier,
        [Type<IdentifierAST>]
        [Rule(Terminal.List, AS, VALUE, IDENTITY)]
        [Rule(Terminal.Array, AS, VALUE, IDENTITY)]
        [Rule(Terminal.Identifier, AS, VALUE, IDENTITY)]
        IdentifierOrReserved
    }
    static ListAST<StatementAST> CreateElseIfElsePart(ExpressionAST cond, IdentifierAST? label, ListAST<StatementAST> trueStmts, ListAST<StatementAST> elsePart)
        => [
            new IfStatementAST(cond, trueStmts, elsePart, label)
        ];
    static ListAST<ParameterAST> UntypedToTypedParams(ListAST<IdentifierAST> untypedParams, TypeAST type)
    {
        ListAST<ParameterAST> typedParams = [];
        foreach (var id in untypedParams)
            typedParams.Add(new(id, type));
        return typedParams;
    }
    static ListAST<T> Combine<T>(ListAST<T> list1, ListAST<T> list2)
    {
        foreach (var val in list2)
            list1.Add(val);
        return list1;
    }
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
