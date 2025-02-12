using QuickCode.AST.Classes;
using QuickCode.AST.Expressions;

namespace QuickCode.AST.FileProgram;

public record class QuickCodeNamespaceAST(ListAST<IdentifierAST> Name, ListAST<QuickCodeClassAST> Types) : AST;
