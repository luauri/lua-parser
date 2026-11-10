namespace Parser.AST;

public record FunctionDeclarationNode(
    IdentifierNode Name,
    List<IdentifierNode> Parameters,
    List<ExprNode> Body,
    bool IsLocal,
    bool Isvararg // i couldve made seperate nodes for (local, global) but this is simpler 
) : ExprNode;