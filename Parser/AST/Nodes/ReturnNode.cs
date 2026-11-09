namespace Parser.AST;
public record ReturnNode(List<ExprNode> Values) : ExprNode;