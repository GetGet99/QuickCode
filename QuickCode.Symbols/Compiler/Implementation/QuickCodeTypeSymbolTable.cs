using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols.Compiler.Implementation
{
    /// <summary>
    /// A read-write function symbol table with optional parent lookup.
    /// Allows overloads with same parameters for different names.
    /// Uses dictionaries for faster lookup.
    /// </summary>
    public class QuickCodeTypeSymbolTable : ITypeSymbolTableWriteable
    {
        private readonly Dictionary<string, List<(ITypeSymbol?[] GenericBaseTypes, ITypeSymbol Symbol)>> _types = new();
        
        private readonly ITypeSymbolTable? _parent;

        public QuickCodeTypeSymbolTable(ITypeSymbolTable? parent = null)
        {
            _parent = parent;
        }

        // TypeSymbol get
        ITypeSymbol? ITypeSymbolTable.this[string name, ITypeSymbol[] args]
        {
            get
            {
                var cur = GetInCurrentLevel(name, args);
                if (cur != null)
                {
                    return cur;
                }
                return _parent?[name, args];
            }
        }

        // TypeSymbol set
        ITypeSymbol ITypeSymbolTableWriteable.this[string name]
        {
            set
            {
                if (!_types.TryGetValue(name, out var list))
                {
                    list = new();
                    _types[name] = list;
                }
                if (value == null)
                {

                }
                else
                {
                    foreach (var (generics, _) in list)
                    {
                        if (generics.Length != 0)
                            throw new NotImplementedException();
                        if (generics.Length == 0)
                            throw new InvalidOperationException("Type with the same name and generic parameter count already exists.");
                        //if (generics.Length == value.GenericTypes.Length)
                        //    throw new InvalidOperationException("Type with the same name and generic parameter count already exists.");
                    }
                    list.Add((/* generic params */[], value));
                }
            }
        }

        public ITypeSymbol? GetInCurrentLevel(string name, ITypeSymbol[] args)
        {
            if (_types.TryGetValue(name, out var list))
            {
                foreach (var (generics, s) in list)
                {
                    if (args.Length != generics.Length)
                        continue;
                    for (int i = 0; i < generics.Length; i++)
                    {
                        // TODO: Change to compatible assign
                        if (args[i] != generics[i])
                        {
                            continue;
                        }
                    }
                    return s;
                }
            }
            return null;
        }
    }
}