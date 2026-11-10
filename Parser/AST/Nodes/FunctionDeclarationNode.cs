namespace Parser.AST;

public record FunctionDeclarationNode(
    IdentifierNode Name,
    List<IdentifierNode> Parameters,
    List<ExprNode> Body,
    bool IsLocal,
    bool Isvararg 
) : ExprNode;