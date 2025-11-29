using Mono.Cecil.Cil;
using QuickCode.Symbols;
using QuickCode.Symbols.Compiler.Implementation;
using QuickCode.Symbols.Operators;
using QuickCode.Symbols.SymbolTables;
using System.Diagnostics;

namespace QuickCode.Compiler.Symbols.Primitives;

class MSILBool : MSILPrimitiveType
{
    public override ITypeSymbol BaseType { get; }


    // empty
    public override IFieldSymbolTable Fields => field ??= new QuickCodeFieldSymbolTable();
    public override IConstructorSymbolTable Constructors => field ??= new QuickCodeConstructorSymbolTable();
    public override ITypeSymbolTable Types => field ??= new QuickCodeTypeSymbolTable();

    public override IFuncSymbolTable Functions => field ??= CreateFuncs();
    MSILTypeFactory factory;
    public MSILBool(MSILTypeFactory factory) : base(factory.MSILTypeSystem.String)
    {
        BaseType = factory.Object;
        this.factory = factory;
    }
    IFuncSymbolTable CreateFuncs()
    {
        IFuncSymbolTableWritable funcs = new QuickCodeFuncSymbolTable();
        foreach (var method in factory.MSILTypeSystem.Boolean.Resolve().Methods)
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
        }
        funcs.Add(BBB(BinaryOperators.Equal, factory, OpCodes.Ceq));
        funcs.Add(BBB(BinaryOperators.NotEqual, factory, NotEqual));
        // todo: short circuit
        funcs.Add(BBB(BinaryOperators.And, factory, OpCodes.And));
        funcs.Add(BBB(BinaryOperators.Or, factory, OpCodes.Or));
        static void NotEqual(CodeGenUtils u)
        {
            u.IL.Emit(OpCodes.Ceq);
            u.IL.Emit(OpCodes.Ldc_I4, 0);
            u.IL.Emit(OpCodes.Ceq); // val == 0 ? 1 : 0 // invert if
        }
        return funcs;
    }

    NativeBinaryImplement BBB(BinaryOperators op, MSILTypeFactory fac, OpCode opCode) => new()
    {
        Operator = op,
        LeftInputType = this,
        RightInputType = this,
        ReturnType = this,
        DefiningType = this,
        CodeGenAction = (u) =>
        {
            u.IL.Emit(opCode);
        }
    };
    NativeBinaryImplement BBB(BinaryOperators op, MSILTypeFactory fac, Action<CodeGenUtils> act) => new()
    {
        Operator = op,
        LeftInputType = this,
        RightInputType = this,
        ReturnType = this,
        DefiningType = this,
        CodeGenAction = act
    };
}
