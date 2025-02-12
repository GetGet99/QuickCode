using Mono.Cecil.Cil;
using QuickCode.AST.Expressions;
using QuickCode.Symbols;
using QuickCode.MSIL;

namespace QuickCode.Compiler;

public class QuickCodeDefaultSymbols : SymbolTable
{
    public static QuickCodeDefaultSymbols Singleton { get; } = new QuickCodeDefaultSymbols();
    private QuickCodeDefaultSymbols()
    {
        this["int"] = TypeSymbol.Int32;
        this["object"] = TypeSymbol.Object;
        this["bool"] = TypeSymbol.Boolean;
        this["void"] = TypeSymbol.Void;
        this["Print"] = new OverloadedFuncSymbol([
            PrintIntFuncSymbol.Singleton,
            PrintBoolFuncSymbol.Singleton,
            PrintStringFuncSymbol.Singleton
        ]);
        TypeSymbol.Int32.Children[UnaryOperators.Identity] = II(u => { /* no-op */ });
        TypeSymbol.Int32.Children[UnaryOperators.Negate] = II(OpCodes.Neg);
        TypeSymbol.Int32.Children[BinaryOperators.Add] = III(OpCodes.Add);
        TypeSymbol.Int32.Children[BinaryOperators.Subtract] = III(OpCodes.Sub);
        TypeSymbol.Int32.Children[BinaryOperators.Multiply] = III(OpCodes.Mul);
        TypeSymbol.Int32.Children[BinaryOperators.Divide] = III(OpCodes.Div);
        TypeSymbol.Int32.Children[BinaryOperators.Modulo] = III(OpCodes.Rem);
        TypeSymbol.Int32.Children[BinaryOperators.MoreThan] = IIB(OpCodes.Cgt);
        TypeSymbol.Int32.Children[BinaryOperators.MoreThanOrEqual] = IIB(u =>
        {
            var v1 = u.IL.DeclareLocal(u.GetTypeRef(TypeSymbol.Int32));
            var v2 = u.IL.DeclareLocal(u.GetTypeRef(TypeSymbol.Int32));
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
        });
        TypeSymbol.Int32.Children[BinaryOperators.LessThan] = IIB(OpCodes.Clt);
        TypeSymbol.Int32.Children[BinaryOperators.LessThanOrEqual] = IIB(u =>
        {
            var v1 = u.IL.DeclareLocal(u.GetTypeRef(TypeSymbol.Int32));
            var v2 = u.IL.DeclareLocal(u.GetTypeRef(TypeSymbol.Int32));
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
        });
        TypeSymbol.Int32.Children[BinaryOperators.Equal] = IIB(OpCodes.Ceq);
        TypeSymbol.Int32.Children[BinaryOperators.NotEqual] = IIB(NotEqual);

        TypeSymbol.Boolean.Children[BinaryOperators.Equal] = BBB(OpCodes.Ceq);
        TypeSymbol.Boolean.Children[BinaryOperators.NotEqual] = BBB(NotEqual);
        TypeSymbol.Boolean.Children[UnaryOperators.Not] = BB(u =>
        {
            u.IL.Emit(OpCodes.Ldc_I4, 0);
            u.IL.Emit(OpCodes.Ceq); // val == 0 ? 1 : 0
        });
        // since booleans are just int 1 and 0, it is equivalent to bitwise version
        // to do: short circuit
        TypeSymbol.Boolean.Children[BinaryOperators.And] = BBB(OpCodes.And);
        TypeSymbol.Boolean.Children[BinaryOperators.Or] = BBB(OpCodes.Or);

        TypeSymbol.String.Children[BinaryOperators.Add] = SSS(u =>
        {
            var method = u.Module.TypeSystem.String.Resolve().Methods.First(
                m => m.Name is "Concat" &&
                m.Parameters.Count is 2 &&
                m.Parameters[0].ParameterType.FullName is "System.String" &&
                m.Parameters[1].ParameterType.FullName is "System.String" &&
                m.IsStatic
            );
            u.IL.Emit(OpCodes.Call, u.Module.ImportReference(method));
        });
        TypeSymbol.String.Children[BinaryOperators.Equal] = SSB(u =>
        {
            var method = u.Module.TypeSystem.String.Resolve().Methods.First(
                m => m.Name is "Equals" &&
                m.Parameters.Count is 2 &&
                m.Parameters[0].ParameterType.FullName is "System.String" &&
                m.Parameters[1].ParameterType.FullName is "System.String" &&
                m.IsStatic
            );
            u.IL.Emit(OpCodes.Call, u.Module.ImportReference(method));
        });

        static void NotEqual(CodeGenUtils u)
        {
            u.IL.Emit(OpCodes.Ceq);
            u.IL.Emit(OpCodes.Ldc_I4, 0);
            u.IL.Emit(OpCodes.Ceq); // val == 0 ? 1 : 0 // invert if
        }
    }
    static NativeBinaryOpInst SSS(Action<CodeGenUtils> act) => new(act, TypeSymbol.String, TypeSymbol.String, TypeSymbol.String);
    static NativeBinaryOpInst SSB(Action<CodeGenUtils> act) => new(act, TypeSymbol.String, TypeSymbol.String, TypeSymbol.Boolean);
    static NativeBinaryOpInst III(OpCode opcode) => new(opcode, TypeSymbol.Int32, TypeSymbol.Int32, TypeSymbol.Int32);
    static NativeBinaryOpInst III(Action<CodeGenUtils> act) => new(act, TypeSymbol.Int32, TypeSymbol.Int32, TypeSymbol.Int32);
    static NativeBinaryOpInst IIB(OpCode opcode) => new(opcode, TypeSymbol.Int32, TypeSymbol.Int32, TypeSymbol.Boolean);
    static NativeBinaryOpInst IIB(Action<CodeGenUtils> act) => new(act, TypeSymbol.Int32, TypeSymbol.Int32, TypeSymbol.Boolean);
    static NativeBinaryOpInst BBB(OpCode opcode) => new(opcode, TypeSymbol.Boolean, TypeSymbol.Boolean, TypeSymbol.Boolean);
    static NativeBinaryOpInst BBB(Action<CodeGenUtils> act) => new(act, TypeSymbol.Boolean, TypeSymbol.Boolean, TypeSymbol.Boolean);
    static NativeUnaryOpInst II(Action<CodeGenUtils> act) => new(act, TypeSymbol.Int32, TypeSymbol.Int32);
    static NativeUnaryOpInst II(OpCode opcode) => new(opcode, TypeSymbol.Int32, TypeSymbol.Int32);
    static NativeUnaryOpInst BB(Action<CodeGenUtils> act) => new(act, TypeSymbol.Boolean, TypeSymbol.Boolean);
}

record class NativeBinaryOpInst(Action<CodeGenUtils> Action, TypeSymbol Left, TypeSymbol Right, TypeSymbol Return) : NativeFuncSymbol([(Left, "left"), (Right, "right")], Return)
{
    public NativeBinaryOpInst(OpCode opcode, TypeSymbol left, TypeSymbol right, TypeSymbol ret) : this(u => u.IL.Emit(opcode), left, right, ret)
    {

    }
    public override void CodeGen(FuncCallAST funcCall, CodeGenUtils u)
    {
        Action(u);
    }
}
record class NativeUnaryOpInst(Action<CodeGenUtils> Action, TypeSymbol Input, TypeSymbol Return) : NativeFuncSymbol([(Input, "")], Return)
{
    public NativeUnaryOpInst(OpCode opcode, TypeSymbol input, TypeSymbol ret) : this(u => u.IL.Emit(opcode), input, ret)
    {

    }
    public override void CodeGen(FuncCallAST funcCall, CodeGenUtils u)
    {
        Action(u);
    }
}