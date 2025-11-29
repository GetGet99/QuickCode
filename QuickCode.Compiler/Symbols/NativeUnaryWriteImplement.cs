using QuickCode.MSIL;
using QuickCode.Symbols;
using QuickCode.Symbols.Functions;
using QuickCode.Symbols.Operators;

namespace QuickCode.Compiler.Symbols;

class NativeUnaryWriteImplement : IUnaryWriteOperatorFuncSymbol, INativeMSILImpl
{
    public required UnaryWriteOperators Operator { get; init; }
    public required ITypeSymbol InputType { get; init; }
    public required ITypeSymbol? DefiningType { get; init; }
    public required ITypeSymbol ReturnType { get; init; }
    public required Action<CodeGenUtils> CodeGenAction { get; init; }
    public void CodeGen(CodeGenUtils u) => CodeGenAction(u);
}
