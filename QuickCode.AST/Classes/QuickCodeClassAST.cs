using QuickCode.AST.FileProgram;

namespace QuickCode.AST.Classes;

public record class QuickCodeClassAST(
    TypeAST Name,
    ListAST<IClassDeclarable> Declarables,
    ListAST<TypeAST> BaseClassOrInterfaces
) : AST
{
    public QuickCodeClassAST(
        TypeAST Name,
        ListAST<IClassDeclarable> Declarables
    ) : this(
        Name, Declarables,
        [new TypeIdentifierAST(new("object"))]
    )
    {

    }
}
