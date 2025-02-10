using QuickCode.AST.Expressions;
using QuickCode.AST.FileProgram;
using QuickCode.AST.Statements;
using QuickCode.Symbols;
using QuickCode.AST.TopLevels;

namespace QuickCode.AST;

public record class FunctionAST(IdentifierAST Name, ListAST<ParameterAST> Parameters, ListAST<StatementAST> Statements, TypeAST? ReturnType = null)
    : AST, ITopLevelDeclarable, IClassDeclarable
{
    public FunctionAST(IdentifierAST Name, ListAST<StatementAST> Statements, TypeAST? ReturnType = null) : this(Name, [], Statements, ReturnType) { }
    /// <summary>
    /// Gets the symbol table for the function.
    /// The symbol table is only valid after type check
    /// </summary>
    public SymbolTable SymbolTable { get; internal set; } = null!;
}
