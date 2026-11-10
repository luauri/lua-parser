namespace Parser.AST;
public record FunctionExpressionNode(
    List<IdentifierNode> Parameters,
    List<ExprNode> Body,
    bool Isvararg
) : ExprNode;