using QuickCode.Symbols.SymbolTables;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickCode.Symbols.Compiler.Implementation
{
    class QuickCodeTypeRefSymbol(ITypeSymbol type) : ITypeSymbol
    {
        public ITypeSymbol ReferencedType { get; } = type;
        public ITypeSymbol BaseType => throw new NotImplementedException();

        public IFieldSymbolTable Fields => throw new NotImplementedException();

        public IFuncSymbolTable Functions => throw new NotImplementedException();

        public IConstructorSymbolTable Constructors => throw new NotImplementedException();

        public ITypeSymbolTable Types => throw new NotImplementedException();
    }
}
