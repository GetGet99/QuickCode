using QuickCode.AST.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace QuickCode.AST.Symbols;

public class SymbolTable(SymbolTable? Parent = null)
{
    public SymbolTable? Parent { get; } = Parent;
    readonly Dictionary<string, Symbol> CurrentSymbols = [];
    public ICollection<string> CurrentLevelSymbolNames => CurrentSymbols.Keys;
    public IEnumerable<KeyValuePair<string, Symbol>> CurrentLevelSymbolKeys()
        => CurrentSymbols;
    /// <summary>
    /// Gets the symbol declared in the symbol tables in any level above current<br/>
    /// or sets the symbol declared in this level.
    /// </summary>
    /// <param name="name">The name of the symbol</param>
    /// <returns>The symbol declared in the symbol tables. If the symbol is not found
    /// at any level, returns null.</returns>
    /// <remarks>
    /// The setter adds the symbol to the current level, while the getter gets
    /// the symbol declared in the current level and all the ascendents.
    /// When using the setter, the symbol must not be null.
    /// </remarks>
    /// <exception cref="ArgumentNullException"/>
    [DisallowNull]
    public Symbol? this[string name]
    {
        get
        {
            if (CurrentSymbols.TryGetValue(name, out var sym))
                return sym;
            return Parent?[name];
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            CurrentSymbols[name] = value;
        }
    }
    /// <inheritdoc cref="this[string]"/>
    [DisallowNull]
    public Symbol? this[BinaryOperators name]
    {
        get => this[Translator(name)];
        set => this[Translator(name)] = value;
    }
    /// <inheritdoc cref="this[string]"/>
    [DisallowNull]
    public Symbol? this[UnaryOperators name]
    {
        get => this[Translator(name)];
        set => this[Translator(name)] = value;
    }
    /// <inheritdoc cref="this[string]"/>
    [DisallowNull]
    public Symbol? this[UnaryWriteOperators name]
    {
        get => this[Translator(name)];
        set => this[Translator(name)] = value;
    }
    static string Translator(BinaryOperators bin)
    {
        return "binary " + bin switch
        {
            BinaryOperators.Add => "+",
            BinaryOperators.Subtract => "-",
            BinaryOperators.Multiply => "*",
            BinaryOperators.Divide => "/",
            BinaryOperators.Modulo => "%",
            BinaryOperators.MoreThan => ">",
            BinaryOperators.LessThan => "<",
            BinaryOperators.MoreThanOrEqual => ">=",
            BinaryOperators.LessThanOrEqual => "<=",
            BinaryOperators.And => "&&",
            BinaryOperators.Or => "||",
            BinaryOperators.Equal => "==",
            BinaryOperators.NotEqual => "!=",
            BinaryOperators.Range => "..",
            _ => throw new NotImplementedException()
        };
    }
    static string Translator(UnaryOperators unary)
    {
        return "unary " + unary switch
        {
            UnaryOperators.Not => "!",
            UnaryOperators.Identity => "+",
            UnaryOperators.Negate => "-",
            _ => throw new NotImplementedException()
        };
    }
    static string Translator(UnaryWriteOperators unaryWrite)
    {
        return "unary " + unaryWrite switch
        {
            UnaryWriteOperators.IncrementAfter or UnaryWriteOperators.IncrementBefore => "++",
            UnaryWriteOperators.DecrementAfter or UnaryWriteOperators.DecrementBefore => "--",
            _ => throw new NotImplementedException()
        };
    }
    public Symbol? GetSymbol(string name, out bool existsInCurrentLevel)
    {
        if (CurrentSymbols.TryGetValue(name, out var sym))
        {
            existsInCurrentLevel = true;
            return sym;
        } else
        {
            existsInCurrentLevel = false;
            return Parent?[name];
        }
    }
    /// <summary>
    /// Gets the symbol only at the current level.
    /// </summary>
    /// <param name="name">The name of the declared level.</param>
    /// <returns>If the symbol exists, returns the symbol. Otherwise, returns null</returns>
    public Symbol? GetAtCurrentLevel(string name)
    {
        if (CurrentSymbols.TryGetValue(name, out var sym))
            return sym;
        return null;
    }
    /// <summary>
    /// Finds if the symbol exists at the current level.
    /// </summary>
    /// <param name="name">The name of the declared level.</param>
    /// <returns>If the symbol exists, returns the symbol. Otherwise, returns null</returns>
    public bool ExistsAtCurrentLevel(string name)
    {
        return CurrentSymbols.ContainsKey(name);
    }
    /// <summary>
    /// Finds if the symbol exists at the current level or all ascendants.
    /// </summary>
    /// <param name="name">The name of the declared level.</param>
    /// <returns>If the symbol exists, returns the symbol. Otherwise, returns null</returns>
    public bool Exists(string name)
    {
        if (ExistsAtCurrentLevel(name))
            return true;
        return Parent is not null && Parent.Exists(name);
    }
    public SymbolTable Clone()
    {
        var cloned = new SymbolTable(Parent);
        foreach (var (k, v) in CurrentSymbols)
            cloned.CurrentSymbols[k] = v;
        return cloned;
    }
}
