using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols.Compiler.Implementation;

/// <summary>
/// A read-write field symbol table with optional parent lookup.
/// </summary>
public class QuickCodeVariableSymbolTable(IVariableSymbolTableWritable? parent = null) : IVariableSymbolTableWritable
{
    private readonly Dictionary<string, VarSymbol> _currentLevel = [];

    public VarSymbol? this[string name]
    {
        // Read: first check current level, then parent if not found
        get
        {
            if (_currentLevel.TryGetValue(name, out var symbol))
                return symbol;
            return parent?[name];
        }

        // Write: only to current level
        set
        {
            if (value == null)
                _currentLevel.Remove(name);
            else
                _currentLevel[name] = value;
        }
    }

    public IEnumerable<(string name, VarSymbol symbol)> CurrentLevel => _currentLevel.Select(x => (x.Key, x.Value));
    public VarSymbol? GetSymbol(string name, out bool curLevel)
    {
        if (_currentLevel.TryGetValue(name, out var symbol))
        {
            curLevel = true;
            return symbol;
        }
        curLevel = false;
        return parent?[name];
    }
    public VarSymbol? GetInCurrentLevel(string name)
    {
        _currentLevel.TryGetValue(name, out var symbol);
        return symbol;
    }

    public IVariableSymbolTableWritable Clone()
    {
        var clone = new QuickCodeVariableSymbolTable(parent);
        foreach (var (name, symbol) in _currentLevel)
        {
            clone[name] = symbol; // Copy current level symbols
        }
        return clone;
    }
}