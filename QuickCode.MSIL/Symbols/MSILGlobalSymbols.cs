using Mono.Cecil;
using QuickCode.Symbols.Compiler.Implementation;

namespace QuickCode.MSIL.Symbols;


public class MSILGlobalSymbols : GlobalSymbols
{
    public MSILGlobalSymbols(ModuleDefinition[] importedModules)
    {
        HashSet<string> DuplicatedKeys = [];
        foreach (var module in importedModules)
        {
            foreach (var reference in module.GetTypes())
            {
                if (!SetSymbolWithNamespaceIfNotExist(reference.FullName, new MSILTypeSymbol(reference)))
                {
                    DuplicatedKeys.Add(reference.FullName);
                }
            }
        }
    }
}