using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace QuickCode.Symbols.Compiler.Implementation;


public class GlobalSymbols : INamespaceSymbol
{
    string? INamespaceSymbol.FullName => null;
    readonly Dictionary<string, ISymbol>.AlternateLookup<ReadOnlySpan<char>> Dict
        = new Dictionary<string, ISymbol>().GetAlternateLookup<ReadOnlySpan<char>>();
    [DisallowNull]
    public ISymbol? this[ReadOnlySpan<char> name]
    {
        get
        {
            return Dict[name];
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (name.Contains('.'))
                throw new ArgumentException($"'{name}' must not contain '.'", nameof(name));
            Debug.Assert(value is ITypeSymbol or INamespaceSymbol);
            Dict[name] = value;
        }
    }
    internal void SetSymbolWithNamespace(string name, ISymbol value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Dict[name] = value;
    }
    internal bool SetSymbolWithNamespaceIfNotExist(string name, ISymbol value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!Dict.ContainsKey(name))
        {
            Dict[name] = value;
            return true;
        } else
        {
            return false;
        }
    }
    public INamespaceSymbol? TryGetOrCreateNamespace(ReadOnlySpan<char> name)
    {
        if (Dict.TryGetValue(name, out var sym))
        {
            if (sym is INamespaceSymbol nsSymbol)
            {
                return nsSymbol;
            }
            // Fail to create namespace: something else already exists
            return null;
        }
        int idx = name.LastIndexOf('.');
        if (idx is -1 || TryGetOrCreateNamespace(name[..idx]) is not null)
        {
            // put the namespace symbol into the dictionary
            NamespaceSymbol nsSym = new(this, new string(name));
            Dict[name] = nsSym;
            return nsSym;
        }
        return null;
    }
    public bool Exists(ReadOnlySpan<char> name)
    {
        return Dict.ContainsKey(name);
    }
}
public static class Extension
{
    public static string AppendNamespaceTo(this INamespaceSymbol ns, string name)
    {
        return ns.FullName is { } str ? $"{str}.{name}" : name;
    }
}
