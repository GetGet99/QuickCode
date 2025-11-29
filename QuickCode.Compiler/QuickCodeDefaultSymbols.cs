using QuickCode.Symbols.SymbolTables;
using QuickCode.Compiler.Symbols;
using QuickCode.Compiler.Symbols.Primitives;
using QuickCode.Symbols.Compiler.Implementation;

namespace QuickCode.Compiler;

public class QuickCodeDefaultSymbols : IScopeSymbolTable
{

    public IFieldSymbolTable Fields => field ??= new QuickCodeFieldSymbolTable();
    public IVariableSymbolTableWritable Variables { get; }
    readonly IFuncSymbolTableWritable funcs = new QuickCodeFuncSymbolTable();
    public IFuncSymbolTable Functions => funcs;
    readonly ITypeSymbolTableWriteable types = new QuickCodeTypeSymbolTable();
    public ITypeSymbolTable Types => types;

    MSILTypeFactory typeFac;
    public QuickCodeDefaultSymbols(MSILTypeFactory typeFac)
    {
        this.typeFac = typeFac;
        Variables = new QuickCodeVariableSymbolTable();
        types["int"] = typeFac.Int32;
        types["string"] = typeFac.String;
        types["object"] = typeFac.Object;
        types["bool"] = typeFac.Boolean;
        types["void"] = typeFac.Void;
        funcs.AddStatic("Print", new PrintIntFuncSymbol(typeFac));
        funcs.AddStatic("Print", new PrintStringFuncSymbol(typeFac));
        funcs.AddStatic("Print", new PrintBoolFuncSymbol(typeFac));
        funcs.AddStatic("Print", new PrintObjectFuncSymbol(typeFac));
    }
    private QuickCodeDefaultSymbols(MSILTypeFactory typeFac, QuickCodeDefaultSymbols clonedFrom)
    {
        this.typeFac = typeFac;
        Variables = clonedFrom.Variables.Clone();
        funcs = clonedFrom.funcs.Clone();
        types = clonedFrom.types;
    }

    public bool CanDeclareVariable(string name)
    {
        if (Variables.CurrentLevel.Any(x => x.name == name))
        {
            return false;
        }
        if (types[name, []] is not null)
        {
            return false;
        }
        if (funcs.ContainsFunctionInCurrentLevel(name))
        {
            return false;
        }
        return true;
    }

    public IScopeSymbolTable Clone()
    {
        return new QuickCodeDefaultSymbols(typeFac, this);
    }
}
