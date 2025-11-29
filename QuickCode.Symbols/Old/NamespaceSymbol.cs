//using QuickCode.Symbols.Types;
//using System.Diagnostics.CodeAnalysis;

//namespace QuickCode.Symbols;

//public record class NamespaceSymbol(GlobalSymbols GlobalSymbols, string FullName) : ISymbol, INamespaceSymbol
//{
//    [DisallowNull]
//    public ISymbol? this[ReadOnlySpan<char> name]
//    {
//        get => GlobalSymbols[$"{FullName}.{name}"];
//        set
//        {
//            if (name.Contains('.'))
//                throw new ArgumentException($"'{name}' must not contain '.'", nameof(name));
//            GlobalSymbols.SetSymoblWithNamespace($"{FullName}.{name}", value);
//        }
//    }
//    public bool Exists(ReadOnlySpan<char> name)
//    {
//        return GlobalSymbols.Exists($"{FullName}.{name}");
//    }
//    public NamespaceSymbol? TryGetOrCreateNamespace(ReadOnlySpan<char> name)
//    {
//        return GlobalSymbols.TryGetOrCreateNamespace($"{FullName}.{name}");
//    }
//}
