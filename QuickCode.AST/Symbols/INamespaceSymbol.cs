using System.Diagnostics.CodeAnalysis;

namespace QuickCode.AST.Symbols;

public interface INamespaceSymbol
{
    string? FullName { get; }
    /// <summary>
    /// Gets the symbol declared in the namespace.
    /// </summary>
    /// <param name="name">The name of the symbol</param>
    /// <returns>The symbol declared in the symbol tables. If the symbol is not found
    /// at any level, returns null.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    [DisallowNull]
    Symbol? this[ReadOnlySpan<char> name] { get; set; }
    /// <summary>
    /// Finds if the symbol exists.
    /// </summary>
    /// <param name="name">The name of the declared level.</param>
    /// <returns>If the symbol exists, returns the symbol. Otherwise, returns null</returns>
    bool Exists(ReadOnlySpan<char> name);
    NamespaceSymbol? TryGetOrCreateNamespace(ReadOnlySpan<char> name);
}