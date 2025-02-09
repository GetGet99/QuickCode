using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace QuickCode.AST.Symbols;
public class GlobalSymbols : INamespaceSymbol
{
    string? INamespaceSymbol.FullName => null;
    readonly Dictionary<string, Symbol>.AlternateLookup<ReadOnlySpan<char>> Dict
        = new Dictionary<string, Symbol>().GetAlternateLookup<ReadOnlySpan<char>>();
    [DisallowNull]
    public Symbol? this[ReadOnlySpan<char> name]
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
            Debug.Assert(value is TypeSymbol or NamespaceSymbol);
            Dict[name] = value;
        }
    }
    internal void SetSymoblWithNamespace(string name, Symbol value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Dict[name] = value;
    }
    public NamespaceSymbol? TryGetOrCreateNamespace(ReadOnlySpan<char> name)
    {
        if (Dict.TryGetValue(name, out var sym))
        {
            if (sym is NamespaceSymbol nsSymbol)
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