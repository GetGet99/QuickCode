using System.Diagnostics.CodeAnalysis;

namespace QuickCode.Symbols.Compiler.Implementation;

public record class NamespaceSymbol(GlobalSymbols GlobalSymbols, string FullName) : ISymbol, INamespaceSymbol
{
    [DisallowNull]
    public ISymbol? this[ReadOnlySpan<char> name]
    {
        get => GlobalSymbols[$"{FullName}.{name}"];
        set
        {
            if (name.Contains('.'))
                throw new ArgumentException($"'{name}' must not contain '.'", nameof(name));
            GlobalSymbols.SetSymbolWithNamespace($"{FullName}.{name}", value);
        }
    }
    public bool Exists(ReadOnlySpan<char> name)
    {
        return GlobalSymbols.Exists($"{FullName}.{name}");
    }
    public INamespaceSymbol? TryGetOrCreateNamespace(ReadOnlySpan<char> name)
    {
        return GlobalSymbols.TryGetOrCreateNamespace($"{FullName}.{name}");
    }
}
