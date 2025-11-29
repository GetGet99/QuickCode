using QuickCode.Symbols.Compiler;

namespace QuickCode.Symbols.SymbolTables;

public interface IVariableSymbolTableWritable
{
    VarSymbol? this[string name] { get; set; }
    VarSymbol? GetSymbol(string name, out bool curLevel);
    IEnumerable<(string name, VarSymbol symbol)> CurrentLevel { get; }
    IVariableSymbolTableWritable Clone();
}
