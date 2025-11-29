using QuickCode.Symbols.Functions;
using QuickCode.Symbols.Operators;

namespace QuickCode.Symbols.SymbolTables;

public interface IFuncSymbolTable
{
    IFuncSymbol? GetInstance(string name, ArgumentInfo[] args);
    IFuncSymbol? GetStatic(string name, ArgumentInfo[] args);
    IBinaryOperatorFuncSymbol? this[BinaryOperators binary, ITypeSymbol left, ITypeSymbol right] { get; }
    IUnaryOperatorFuncSymbol? this[UnaryOperators unary, ITypeSymbol target] { get; }
    IUnaryWriteOperatorFuncSymbol? this[UnaryWriteOperators unaryWrite, ITypeSymbol target] { get; }

    bool ContainsFunctionInCurrentLevel(string name);
    IEnumerable<IFuncSymbol> CurrentLevel { get; }
    IEnumerable<IBinaryOperatorFuncSymbol> CurrentLevelBinaryOperators { get; }
    IEnumerable<IUnaryOperatorFuncSymbol> CurrentLevelUnaryOperators { get; }
    IEnumerable<IUnaryWriteOperatorFuncSymbol> CurrentLevelUnaryWriteOperators { get; }
}
public interface IFuncSymbolTableWritable : IFuncSymbolTable
{
    void AddInstance(string name, IFuncSymbol? funcSymbol);
    void AddStatic(string name, IFuncSymbol? funcSymbol);
    void Add(IBinaryOperatorFuncSymbol binary);
    void Add(IUnaryOperatorFuncSymbol unary);
    void Add(IUnaryWriteOperatorFuncSymbol unaryWrite);
    IFuncSymbolTableWritable Clone();
}
