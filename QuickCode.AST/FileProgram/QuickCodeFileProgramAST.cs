namespace QuickCode.AST.FileProgram;

public record class QuickCodeFileProgramAST(ListAST<QuickCodeNamespaceAST> Namespaces) : QuickCodeAST;
