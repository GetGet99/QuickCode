using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols.Compiler.Implementation;

/// <summary>
/// A read-write field symbol table with optional parent lookup.
/// </summary>
public class QuickCodeFieldSymbolTable(IFieldSymbolTable? parent = null) : IFieldSymbolTableWritable
{
    private readonly Dictionary<string, IFieldSymbol> _currentLevel = [];

    // Read: first check current level, then parent if not found
    IFieldSymbol? IFieldSymbolTable.this[string name]
    {
        get
        {
            if (_currentLevel.TryGetValue(name, out var symbol))
                return symbol;
            return parent?.GetInCurrentLevel(name);
        }
    }

    // Write: only to current level
    IFieldSymbol? IFieldSymbolTableWritable.this[string name]
    {
        set
        {
            if (value == null)
                _currentLevel.Remove(name);
            else
                _currentLevel[name] = value;
        }
    }

    public IEnumerable<IFieldSymbol> CurrentLevel => _currentLevel.Values;

    public IFieldSymbol? GetInCurrentLevel(string name)
    {
        _currentLevel.TryGetValue(name, out var symbol);
        return symbol;
    }
}