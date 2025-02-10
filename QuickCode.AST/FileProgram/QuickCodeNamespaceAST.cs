using QuickCode.AST.Classes;

namespace QuickCode.AST.FileProgram;

public record class QuickCodeNamespaceAST(ListAST<QuickCodeClassAST> Classes);
