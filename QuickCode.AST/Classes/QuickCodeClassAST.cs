using QuickCode.AST.FileProgram;
using QuickCode.AST.TopLevels;

namespace QuickCode.AST.Classes;

public record class QuickCodeClassAST(
    TypeAST Name,
    ListAST<IClassDeclarable> Declarables,
    ListAST<TypeAST> BaseClassOrInterfaces
) : AST, ITopLevelDeclarable
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
