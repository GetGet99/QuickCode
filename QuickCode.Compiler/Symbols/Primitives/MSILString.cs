using Mono.Cecil.Cil;
using QuickCode.Symbols;
using QuickCode.Symbols.Compiler.Implementation;
using QuickCode.Symbols.Operators;
using QuickCode.Symbols.SymbolTables;
using System.Diagnostics;

namespace QuickCode.Compiler.Symbols.Primitives;

class MSILString : MSILPrimitiveType
{
    public override ITypeSymbol BaseType { get; }


    // empty
    public override IFieldSymbolTable Fields => field ??= new QuickCodeFieldSymbolTable();
    public override IConstructorSymbolTable Constructors => field ??= new QuickCodeConstructorSymbolTable();
    public override ITypeSymbolTable Types => field ??= new QuickCodeTypeSymbolTable();


    public override IFuncSymbolTable Functions => field ??= CreateFuncs();
    MSILTypeFactory factory;
    public MSILString(MSILTypeFactory factory) : base(factory.MSILTypeSystem.String)
    {
        BaseType = factory.Object;
        this.factory = factory;
    }
    IFuncSymbolTable CreateFuncs()
    {
        IFuncSymbolTableWritable funcs = new QuickCodeFuncSymbolTable();
        foreach (var method in factory.MSILTypeSystem.String.Resolve().Methods)
        {
            if (method.IsSpecialName) continue;
            try
            {
                var wrapped = new MSILMethodWrapper(factory, method);
                if (method.IsStatic)
                {
                    funcs.AddStatic(method.Name, wrapped);
                }
                else
                {
                    funcs.AddInstance(method.Name, wrapped);
                }
            }
            catch (NotImplementedException)
            {
                // not supported yet
            }
        };
        var stringEquals = factory.MSILTypeSystem.String.Resolve().Methods.First(
                m => m.Name is "Equals" &&
                m.Parameters.Count is 2 &&
                m.Parameters[0].ParameterType.FullName is "System.String" &&
                m.Parameters[1].ParameterType.FullName is "System.String" &&
                m.IsStatic
            );
        var stringCat = factory.MSILTypeSystem.String.Resolve().Methods.First(
                m => m.Name is "Concat" &&
                m.Parameters.Count is 2 &&
                m.Parameters[0].ParameterType.FullName is "System.String" &&
                m.Parameters[1].ParameterType.FullName is "System.String" &&
                m.IsStatic
            );
        funcs.Add(SSB(BinaryOperators.Equal, factory, u =>
        {
            u.IL.Emit(OpCodes.Call, u.Module.ImportReference(stringEquals));
        }));
        funcs.Add(SSB(BinaryOperators.NotEqual, factory, u =>
        {
            u.IL.Emit(OpCodes.Call, u.Module.ImportReference(stringEquals));
            u.IL.Emit(OpCodes.Ldc_I4, 0);
            u.IL.Emit(OpCodes.Ceq); // string.Equals(...) == 0 ? 1 : 0 // invert if
        }));
        funcs.Add(SSS(BinaryOperators.Add, factory, u =>
        {
            u.IL.Emit(OpCodes.Call, u.Module.ImportReference(stringCat));
        }));
        return funcs;
    }
    NativeBinaryImplement SSB(BinaryOperators op, MSILTypeFactory fac, Action<CodeGenUtils> act) => new()
    {
        Operator = op,
        LeftInputType = this,
        RightInputType = this,
        ReturnType = fac.Boolean,
        DefiningType = this,
        CodeGenAction = act
    };
    NativeBinaryImplement SSS(BinaryOperators op, MSILTypeFactory fac, Action<CodeGenUtils> act) => new()
    {
        Operator = op,
        LeftInputType = this,
        RightInputType = this,
        ReturnType = this,
        DefiningType = this,
        CodeGenAction = act
    };
}
