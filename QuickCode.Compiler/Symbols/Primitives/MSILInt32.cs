using Mono.Cecil.Cil;
using QuickCode.MSIL;
using QuickCode.Symbols;
using QuickCode.Symbols.Compiler.Implementation;
using QuickCode.Symbols.Operators;
using QuickCode.Symbols.SymbolTables;
using System.Diagnostics;

namespace QuickCode.Compiler.Symbols.Primitives;

class MSILInt32 : MSILPrimitiveType
{
    public override ITypeSymbol BaseType { get; }

    // empty
    public override IFieldSymbolTable Fields => field ??= new QuickCodeFieldSymbolTable();
    public override IConstructorSymbolTable Constructors => field ??= new QuickCodeConstructorSymbolTable();
    public override ITypeSymbolTable Types => field ??= new QuickCodeTypeSymbolTable();

    public override IFuncSymbolTable Functions => field ??= CreateFuncs();
    MSILTypeFactory factory;
    public MSILInt32(MSILTypeFactory factory) : base(factory.MSILTypeSystem.String)
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
        funcs.Add(II(UnaryOperators.Identity, factory, u => { /* no-op */ }));
        funcs.Add(II(UnaryOperators.Negate, factory, OpCodes.Neg));
        funcs.Add(III(BinaryOperators.Add, factory, OpCodes.Add));
        funcs.Add(III(BinaryOperators.Subtract, factory, OpCodes.Sub));
        funcs.Add(III(BinaryOperators.Multiply, factory, OpCodes.Mul));
        funcs.Add(III(BinaryOperators.Divide, factory, OpCodes.Div));
        funcs.Add(III(BinaryOperators.Modulo, factory, OpCodes.Rem));
        funcs.Add(IIB(BinaryOperators.MoreThan, factory, OpCodes.Cgt));
        funcs.Add(IIB(BinaryOperators.MoreThanOrEqual, factory, u =>
        {
            var v1 = u.IL.DeclareLocal(MSIL.Extension.GetTypeRef(u, factory.Int32));
            var v2 = u.IL.DeclareLocal(MSIL.Extension.GetTypeRef(u, factory.Int32));
            // store them first
            u.IL.Emit(OpCodes.Stloc, v2);
            u.IL.Emit(OpCodes.Stloc, v1);
            // use them twice
            u.IL.Emit(OpCodes.Ldloc, v1);
            u.IL.Emit(OpCodes.Ldloc, v2);
            u.IL.Emit(OpCodes.Cgt);
            u.IL.Emit(OpCodes.Ldloc, v1);
            u.IL.Emit(OpCodes.Ldloc, v2);
            u.IL.Emit(OpCodes.Ceq);
            u.IL.Emit(OpCodes.Or);
        }));
        funcs.Add(IIB(BinaryOperators.LessThan, factory, OpCodes.Clt));
        funcs.Add(IIB(BinaryOperators.LessThanOrEqual, factory, u =>
        {
            var v1 = u.IL.DeclareLocal(MSIL.Extension.GetTypeRef(u, factory.Int32));
            var v2 = u.IL.DeclareLocal(MSIL.Extension.GetTypeRef(u, factory.Int32));
            // store them first
            u.IL.Emit(OpCodes.Stloc, v2);
            u.IL.Emit(OpCodes.Stloc, v1);
            // use them twice
            u.IL.Emit(OpCodes.Ldloc, v1);
            u.IL.Emit(OpCodes.Ldloc, v2);
            u.IL.Emit(OpCodes.Clt);
            u.IL.Emit(OpCodes.Ldloc, v1);
            u.IL.Emit(OpCodes.Ldloc, v2);
            u.IL.Emit(OpCodes.Ceq);
            u.IL.Emit(OpCodes.Or);
        }));
        funcs.Add(IIB(BinaryOperators.Equal, factory, OpCodes.Ceq));
        funcs.Add(IIB(BinaryOperators.NotEqual, factory, NotEqual));
        funcs.Add(II(UnaryWriteOperators.IncrementBefore, factory, u =>
        {
            u.IL.Emit(OpCodes.Ldc_I4, 1);
            u.IL.Emit(OpCodes.Add);
        }));
        funcs.Add(II(UnaryWriteOperators.IncrementAfter, factory, u =>
        {
            u.IL.Emit(OpCodes.Ldc_I4, 1);
            u.IL.Emit(OpCodes.Add);
        }));
        funcs.Add(II(UnaryWriteOperators.DecrementBefore, factory, u =>
        {
            u.IL.Emit(OpCodes.Ldc_I4, 1);
            u.IL.Emit(OpCodes.Sub);
        }));
        funcs.Add(II(UnaryWriteOperators.DecrementAfter, factory, u =>
        {
            u.IL.Emit(OpCodes.Ldc_I4, 1);
            u.IL.Emit(OpCodes.Sub);
        }));
        static void NotEqual(CodeGenUtils u)
        {
            u.IL.Emit(OpCodes.Ceq);
            u.IL.Emit(OpCodes.Ldc_I4, 0);
            u.IL.Emit(OpCodes.Ceq); // val == 0 ? 1 : 0 // invert if
        }
        return funcs;
    }

    NativeUnaryImplement II(UnaryOperators op, MSILTypeFactory fac, OpCode opCode) => new()
    {
        Operator = op,
        InputType = this,
        ReturnType = this,
        DefiningType = this,
        CodeGenAction = (u) =>
        {
            u.IL.Emit(opCode);
        }
    };
    NativeUnaryImplement II(UnaryOperators op, MSILTypeFactory fac, Action<CodeGenUtils> act) => new()
    {
        Operator = op,
        InputType = this,
        ReturnType = this,
        DefiningType = this,
        CodeGenAction = act
    };
    NativeUnaryWriteImplement II(UnaryWriteOperators op, MSILTypeFactory fac, Action<CodeGenUtils> act) => new()
    {
        Operator = op,
        InputType = this,
        ReturnType = this,
        DefiningType = this,
        CodeGenAction = act
    };
    NativeBinaryImplement III(BinaryOperators op, MSILTypeFactory fac, OpCode opCode) => new()
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
    NativeBinaryImplement IIB(BinaryOperators op, MSILTypeFactory fac, OpCode opCode) => new()
    {
        Operator = op,
        LeftInputType = this,
        RightInputType = this,
        ReturnType = fac.Boolean,
        DefiningType = this,
        CodeGenAction = (u) =>
        {
            u.IL.Emit(opCode);
        }
    };
    NativeBinaryImplement IIB(BinaryOperators op, MSILTypeFactory fac, Action<CodeGenUtils> act) => new()
    {
        Operator = op,
        LeftInputType = this,
        RightInputType = this,
        ReturnType = fac.Boolean,
        DefiningType = this,
        CodeGenAction = act
    };

}
