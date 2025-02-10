using QuickCode.AST;
using QuickCode.AST.Expressions;
using QuickCode.AST.Statements;
using QuickCode.AST.Symbols;
using QuickCode.AST.TopLevels;
using System.Diagnostics.CodeAnalysis;

namespace QuickCode.Flatten;

public class QuickCodeFlatten(SymbolTable symbolTable)
{
    SymbolTable SymbolTable { get; } = symbolTable;
    ListAST<StatementAST> StatementList = [];
    public TopLevelProgramAST Flatten(TopLevelProgramAST program)
    {
        StatementList = [];
        Flatten
        return new();
    }
    IEnumerable<TopLevelProgramComponentAST> Flatten(TopLevelProgramComponentAST program)
    {
        switch (program)
        {
            case TopLevelStatementAST statement:
                Flatten(statement.Statement);
                break;
            default:
                break;
        }
    }
    IEnumerable<StatementAST> Flatten(StatementAST statement)
    {
        switch (statement)
        {
            case ExprStatementAST expr:
                break;
            case LocalVarDeclStatementAST localVarDeclStatement:
                break;
            default:
                break;
        }
    }
    IEnumerable<StatementAST> Flatten(ExpressionAST expr)
    {
        switch (expr)
        {
            case BinaryAST binary:
                
                break;
            case LocalVarDeclStatementAST localVarDeclStatement:
                break;
            default:
                break;
        }
    }
    (ExpressionAST replacement, IEnumerable<StatementAST>) Flatten(BinaryAST binary)
    {
        StatementList.
    }
    [DoesNotReturn]
    static void NotImplemented(AST.AST node)
    {
        throw new NotImplementedException($"The node of type {node.GetType().Name} is not implemented.");
    }
}
