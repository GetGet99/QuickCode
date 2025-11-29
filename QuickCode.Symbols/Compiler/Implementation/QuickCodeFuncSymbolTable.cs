using System;
using System.Collections.Generic;
using QuickCode.Symbols.Functions;
using QuickCode.Symbols.Operators;
using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols.Compiler.Implementation
{
    /// <summary>
    /// A read-write function symbol table with optional parent lookup.
    /// Allows overloads with same parameters for different names.
    /// Uses dictionaries for faster lookup.
    /// </summary>
    public class QuickCodeFuncSymbolTable : IFuncSymbolTableWritable
    {
        // Functions: key is function name, value is list of (args, symbol)
        private readonly Dictionary<string, List<(ArgumentInfo[] Args, IFuncSymbol Symbol)>> _funcs;
        // Functions: key is function name, value is list of (args, symbol)
        private readonly Dictionary<string, List<(ArgumentInfo[] Args, IFuncSymbol Symbol)>> _staticFuncs;
        // Binary operators: key is operator, value is list of (left, right, symbol)
        private readonly Dictionary<BinaryOperators, List<(ITypeSymbol Left, ITypeSymbol Right, IBinaryOperatorFuncSymbol Symbol)>> _binaryOps;
        // Unary operators: key is operator, value is list of (target, symbol)
        private readonly Dictionary<UnaryOperators, List<(ITypeSymbol Target, IUnaryOperatorFuncSymbol Symbol)>> _unaryOps;
        // Unary write operators: key is operator, value is list of (target, symbol)
        private readonly Dictionary<UnaryWriteOperators, List<(ITypeSymbol Target, IUnaryWriteOperatorFuncSymbol Symbol)>> _unaryWriteOps;

        private readonly IFuncSymbolTable? _parent;

        public QuickCodeFuncSymbolTable(IFuncSymbolTable? parent = null)
        {
            _parent = parent;
            _funcs = [];
            _staticFuncs = [];
            _binaryOps = [];
            _unaryOps = [];
            _unaryWriteOps = [];
        }
        private QuickCodeFuncSymbolTable(QuickCodeFuncSymbolTable other)
        {
            _parent = other._parent;
            _funcs = other._funcs.Clone();
            _staticFuncs = other._staticFuncs.Clone();
            _binaryOps = other._binaryOps.Clone();
            _unaryOps = other._unaryOps.Clone();
            _unaryWriteOps = other._unaryWriteOps.Clone();
        }

        // Helper: compare ArgumentInfo[] by type reference
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

        // Function get
        IFuncSymbol? IFuncSymbolTable.GetInstance(string name, ArgumentInfo[] args)
        {
            if (_funcs.TryGetValue(name, out var list))
            {
                foreach (var (a, s) in list)
                {
                    if (ArgsEqualByTypeRef(a, args))
                        return s;
                }
            }
            return _parent?.GetInstance(name, args);
        }
        IFuncSymbol? IFuncSymbolTable.GetStatic(string name, ArgumentInfo[] args)
        {
            if (_staticFuncs.TryGetValue(name, out var list))
            {
                foreach (var (a, s) in list)
                {
                    if (ArgsEqualByTypeRef(a, args))
                        return s;
                }
            }
            return _parent?.GetStatic(name, args);
        }

        // Function set
        void IFuncSymbolTableWritable.AddInstance(string name, IFuncSymbol? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var parameters = value.Parameters;
            if (!_funcs.TryGetValue(name, out var list))
            {
                list = [];
                _funcs[name] = list;
            }
            var args = parameters.Select(x => new ArgumentInfo(x.Name, x.Type)).ToArray();
            if (parameters.Any(x => x.HasDefaultParameter))
            {
                throw new NotImplementedException();
            }
            foreach (var (a, _) in list)
            {
                if (ArgsEqualByTypeRef(a, args))
                    throw new InvalidOperationException("Function with the same name and parameter types already exists.");
            }
            list.Add((args, value));
        }
        void IFuncSymbolTableWritable.AddStatic(string name, IFuncSymbol? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var parameters = value.Parameters;
            if (!_staticFuncs.TryGetValue(name, out var list))
            {
                list = [];
                _staticFuncs[name] = list;
            }
            var args = parameters.Select(x => new ArgumentInfo(x.Name, x.Type)).ToArray();
            if (parameters.Any(x => x.HasDefaultParameter))
            {
                throw new NotImplementedException();
            }
            foreach (var (a, _) in list)
            {
                if (ArgsEqualByTypeRef(a, args))
                    throw new InvalidOperationException("Function with the same name and parameter types already exists.");
            }
            list.Add((args, value));
        }

        // Binary operator get
        IBinaryOperatorFuncSymbol? IFuncSymbolTable.this[BinaryOperators binary, ITypeSymbol left, ITypeSymbol right]
        {
            get
            {
                if (_binaryOps.TryGetValue(binary, out var list))
                {
                    foreach (var (l, r, s) in list)
                    {
                        if (Equals(l, left) && Equals(r, right))
                            return s;
                    }
                }
                return _parent?[binary, left, right];
            }
        }

        // Binary operator set
        void IFuncSymbolTableWritable.Add(IBinaryOperatorFuncSymbol value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var binary = value.Operator;
            if (!_binaryOps.TryGetValue(binary, out var list))
            {
                list = new();
                _binaryOps[binary] = list;
            }
            foreach (var (l, r, _) in list)
            {
                if (Equals(l, value.LeftInputType) && Equals(r, value.RightInputType))
                    throw new InvalidOperationException("Binary operator with the same types already exists.");
            }
            list.Add((value.LeftInputType, value.RightInputType, value));
        }

        // Unary operator get
        IUnaryOperatorFuncSymbol? IFuncSymbolTable.this[UnaryOperators unary, ITypeSymbol target]
        {
            get
            {
                if (_unaryOps.TryGetValue(unary, out var list))
                {
                    foreach (var (t, s) in list)
                    {
                        if (Equals(t, target))
                            return s;
                    }
                }
                return _parent?[unary, target];
            }
        }

        // Unary operator set
        void IFuncSymbolTableWritable.Add(IUnaryOperatorFuncSymbol value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var unary = value.Operator;
            if (!_unaryOps.TryGetValue(unary, out var list))
            {
                list = new();
                _unaryOps[unary] = list;
            }
            foreach (var (t, _) in list)
            {
                if (Equals(t, value.InputType))
                    throw new InvalidOperationException("Unary operator with the same type already exists.");
            }
            list.Add((value.InputType, value));
        }

        // Unary write operator get
        IUnaryWriteOperatorFuncSymbol? IFuncSymbolTable.this[UnaryWriteOperators unaryWrite, ITypeSymbol target]
        {
            get
            {
                if (_unaryWriteOps.TryGetValue(unaryWrite, out var list))
                {
                    foreach (var (t, s) in list)
                    {
                        if (Equals(t, target))
                            return s;
                    }
                }
                return _parent?[unaryWrite, target];
            }
        }

        // Unary write operator set
        void IFuncSymbolTableWritable.Add(IUnaryWriteOperatorFuncSymbol value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var unaryWrite = value.Operator;
            if (!_unaryWriteOps.TryGetValue(unaryWrite, out var list))
            {
                list = new();
                _unaryWriteOps[unaryWrite] = list;
            }
            foreach (var (t, _) in list)
            {
                if (Equals(t, value.InputType))
                    throw new InvalidOperationException("Unary operator with the same type already exists.");
            }
            list.Add((value.InputType, value));
        }

        public bool ContainsFunctionInCurrentLevel(string name)
        {
            return _funcs.ContainsKey(name) && _funcs[name].Count > 0;
        }

        public IEnumerable<IFuncSymbol> CurrentLevel
        {
            get
            {
                foreach (var list in _funcs.Values)
                    foreach (var (_, symbol) in list)
                        yield return symbol;
            }
        }

        public IEnumerable<IBinaryOperatorFuncSymbol> CurrentLevelBinaryOperators
        {
            get
            {
                foreach (var list in _binaryOps.Values)
                    foreach (var (_, _, symbol) in list)
                        yield return symbol;
            }
        }

        public IEnumerable<IUnaryOperatorFuncSymbol> CurrentLevelUnaryOperators
        {
            get
            {
                foreach (var list in _unaryOps.Values)
                    foreach (var (_, symbol) in list)
                        yield return symbol;
            }
        }

        public IEnumerable<IUnaryWriteOperatorFuncSymbol> CurrentLevelUnaryWriteOperators
        {
            get
            {
                foreach (var list in _unaryWriteOps.Values)
                    foreach (var (_, symbol) in list)
                        yield return symbol;
            }
        }
        public IFuncSymbolTableWritable Clone() => new QuickCodeFuncSymbolTable(this);
    }
}
static class Extension
{
    extension<TKey, TValue>(IDictionary<TKey, TValue> val) where TKey : notnull {
        public Dictionary<TKey, TValue> Clone()
        {
            var newDict = new Dictionary<TKey, TValue>();
            foreach (var key in val.Keys)
            {
                newDict[key] = val[key];
            }
            return newDict;
        }
    }
}