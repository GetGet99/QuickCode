using QuickCode.AST.Expressions;
using QuickCode.AST.FileProgram;
using QuickCode.AST.Statements;
using QuickCode.AST.TopLevels;
using QuickCode.Symbols.Compiler.Implementation;
using QuickCode.Symbols.SymbolTables;

namespace QuickCode.AST;

public record class FunctionAST(IdentifierAST Name, ListAST<ParameterAST> Parameters, ListAST<StatementAST> Statements, TypeAST? ReturnType = null)
    : AST, ITopLevelDeclarable, IClassDeclarable
{
    public FunctionAST(IdentifierAST Name, ListAST<StatementAST> Statements, TypeAST? ReturnType = null) : this(Name, [], Statements, ReturnType) { }
    /// <summary>
    /// Gets the symbol table for the function.
    /// The symbol table is only valid after type check
    /// </summary>
    public IScopeSymbolTable SymbolTable { get; internal set; } = null!;
    /// <summary>
    /// Gets the function symbol
    /// The function symbol is only valid after type check
    /// </summary>
    public QuickCodeFuncSymbol FuncSymbol { get; internal set; } = null!;
    public override int GetHashCode()
    {
        // prevent hash code loop
        return base.GetHashCode();
    }
}
