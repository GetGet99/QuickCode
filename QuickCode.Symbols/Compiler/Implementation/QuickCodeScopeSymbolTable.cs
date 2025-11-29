using QuickCode.Symbols.SymbolTables;

namespace QuickCode.Symbols.Compiler.Implementation;

public class QuickCodeScopeSymbolTable : IScopeSymbolTable
{
    public QuickCodeScopeSymbolTable(ITypeSymbol type, List<INamespaceSymbol> importedNamespaces)
    {
        Fields = type.Fields;
        Variables = new QuickCodeVariableSymbolTable();
        Functions = type.Functions;
        //Constructors = type.Constructors;
        Types = type.Types; // extends this with importedNamespaces
    }
    public QuickCodeScopeSymbolTable(IScopeSymbolTable? parent) : this(parent, false)
    {
        // This constructor is used for creating a new scope based on an existing one, but not cloning the variable table
    }
    private QuickCodeScopeSymbolTable(IScopeSymbolTable? initial, bool cloned)
    {
        if (initial is null)
        {
            if (cloned)
            {
                throw new ArgumentNullException(nameof(initial), "Cannot clone a null scope symbol table.");
            }
            Fields = new QuickCodeFieldSymbolTable(); // Empty field symbol table
            Variables = new QuickCodeVariableSymbolTable();
            Functions = new QuickCodeFuncSymbolTable(); // Empty function symbol table
            Types = new QuickCodeTypeSymbolTable(); // Empty type symbol table
            return;
        }
        Fields = initial.Fields;
        if (!cloned)
            Variables = new QuickCodeVariableSymbolTable(initial.Variables);
        else
            Variables = initial.Variables.Clone();
        Functions = new QuickCodeFuncSymbolTable(initial.Functions);
        //Constructors = type.Constructors;
        Types = initial.Types;
    }
    public IFieldSymbolTable Fields { get; }
    public IVariableSymbolTableWritable Variables { get; }
    public IFuncSymbolTable Functions { get; }
    //public IConstructorSymbolTable Constructors { get; }
    public ITypeSymbolTable Types { get; }

    public bool CanDeclareVariable(string name)
    {
        if (Variables[name] is not null)
        {
            return false; // Variable already declared in the current scope
        }
        return true;
    }

    public IScopeSymbolTable Clone() => new QuickCodeScopeSymbolTable(this, cloned: true);
}
