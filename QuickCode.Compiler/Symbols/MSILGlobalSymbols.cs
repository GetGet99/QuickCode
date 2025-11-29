using Mono.Cecil;
using QuickCode.Symbols;

namespace QuickCode.Compiler.Symbols;

class MSILGlobalSymbols(ModuleDefinition module) : INamespaceSymbol
{
    public ISymbol? this[ReadOnlySpan<char> name] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string? FullName => "";

    public bool Exists(ReadOnlySpan<char> name) => throw new NotImplementedException();

    public INamespaceSymbol? TryGetOrCreateNamespace(ReadOnlySpan<char> name) => throw new NotImplementedException();
}
