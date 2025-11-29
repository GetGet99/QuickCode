using QuickCode.AST.Expressions;
using QuickCode.Symbols;
using QuickCode.Symbols.Functions;

namespace QuickCode.Compiler.Symbols;

public abstract record class NativeConstructorSymbol() : IConstructorSymbol
{
    public abstract ITypeSymbol? DefiningType { get; }
    public abstract ParameterInfo[] Parameters { get; }
    public abstract ITypeSymbol ReturnType { get; }

    /// <summary>
    /// Generaets the code for the given function call.
    /// The arguments are already provided on the stack.
    /// </summary>
    /// <param name="funcCall">The function call AST</param>
    /// <param name="u">The code gen utils</param>
    public abstract void CodeGenCall(FuncCallAST funcCall, CodeGenUtils u);
    /// <summary>
    /// Generaets the code for the given new object call.
    /// The arguments are already provided on the stack.
    /// </summary>
    /// <param name="funcCall">The function call AST</param>
    /// <param name="u">The code gen utils</param>
    public abstract void CodeGenNew(FuncCallAST funcCall, CodeGenUtils u);
}
