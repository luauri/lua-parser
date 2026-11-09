namespace Parser.AST;

public record NumericForNode(
    IdentifierNode Variable,
    ExprNode Start,
    ExprNode End,
    ExprNode? Step, // lua defaults to 1 as step, if it is not provided
    List<ExprNode> Body
) : ExprNode;