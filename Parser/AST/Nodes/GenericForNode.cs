namespace Parser.AST;
public record GenericForNode(
    List<IdentifierNode> Variables,
    List<ExprNode> Iterator, // something like pairs(table)
    List<ExprNode> Body
) : ExprNode;