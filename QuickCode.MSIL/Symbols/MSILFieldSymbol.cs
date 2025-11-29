using Mono.Cecil;
using QuickCode.Symbols;

namespace QuickCode.MSIL.Symbols;

class MSILFieldSymbol(FieldDefinition field) : IFieldSymbol
{
    public ITypeSymbol Type => new MSILTypeSymbol(field.DeclaringType);

    public string Name => field.Name;
}
