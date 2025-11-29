using System;
using System.Collections.Generic;
using QuickCode.Symbols.Functions;
using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols.Compiler.Implementation
{
    /// <summary>
    /// A read-write constructor symbol table with optional parent lookup.
    /// Prevents duplicate constructors with the same parameter types/order.
    /// </summary>
    public class QuickCodeConstructorSymbolTable : IConstructorSymbolTableWritable
    {
        // Store constructors by their argument list for reference equality comparison
        private readonly List<(ArgumentInfo[] Args, IConstructorSymbol Symbol)> _currentLevel = new();
        private readonly IConstructorSymbolTable? _parent;

        public QuickCodeConstructorSymbolTable(IConstructorSymbolTable? parent = null)
        {
            _parent = parent;
        }

        // Helper: compare parameter types by reference equality
        private static bool ArgsEqualByTypeRef(ArgumentInfo[] a, ArgumentInfo[] b)
        {
            if (a == b) return true;
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (!Equals(a[i].Type, b[i].Type))
                    return false;
            }
            return true;
        }

        // Read: first check current level, then parent if not found
        IConstructorSymbol? IConstructorSymbolTable.this[ArgumentInfo[] args]
        {
            get
            {
                foreach (var (storedArgs, symbol) in _currentLevel)
                {
                    if (ArgsEqualByTypeRef(storedArgs, args))
                        return symbol;
                }
                return _parent?[args];
            }
        }

        // Write: only to current level, throw if duplicate parameter types/order (by reference)
        IConstructorSymbol? IConstructorSymbolTableWritable.this[ArgumentInfo[] args]
        {
            set
            {
                if (value == null)
                {
                    _currentLevel.RemoveAll(x => ArgsEqualByTypeRef(x.Args, args));
                }
                else
                {
                    foreach (var (storedArgs, _) in _currentLevel)
                    {
                        if (ArgsEqualByTypeRef(storedArgs, args))
                            throw new InvalidOperationException("Constructor with the same parameter types already exists.");
                    }
                    _currentLevel.Add((args, value));
                }
            }
        }

        public IEnumerable<IConstructorSymbol> Constructors
            => _currentLevel.ConvertAll(x => x.Symbol);
    }
}