using System.Globalization;
namespace Parser.AST;

public static class Extensions
{
    public static string ToFullString(this ExprNode node) // ast -> lua :)
    {
        return node switch
        {
            LocalDeclaration l => $"local {string.Join(", ", l.Names.Select(n => n.ToFullString()))}" + (l.Values.Count > 0 ? $" = {string.Join(", ", l.Values.Select(v => v.ToFullString()))}" : ""),
            IdentifierNode id => id.Name,
            ConstantNode c => c.Kind switch
            {
                Literal.String => $"\"{(string)c.Value}\"", // TODO: add proper string escaping 
                Literal.Boolean => (bool)c.Value ? "true" : "false",
                Literal.Number =>
                    Convert.ToString(c.Value, CultureInfo.InvariantCulture)
                    ?? throw new Exception("invalid number"),
                Literal.Nil => "nil",
                _ => throw new Exception("unknown literal")
            },
            WhileNode w => $"while {w.Condition.ToFullString()} do\n" + FormatBody(w.Body) + "end",

            IfNode i => $"if {i.Condition.ToFullString()} then\n" + FormatBody(i.Body) + string.Concat(i.ElseIfClauses.Select(e => $"elseif {e.Condition.ToFullString()} then\n" + FormatBody(e.Body))) + (i.ElseClause is not null ? "else\n" + FormatBody(i.ElseClause.Body) : "") + "end",

            BinaryExpressionNode b => $"{b.Left.ToFullString()} {b.Operator} {b.Right.ToFullString()}",
            AssignmentNode a => $"{a.Name.ToFullString()} = {a.Value.ToFullString()}",
            CompoundAssignmentNode c => $"{c.Name.ToFullString()} {c.Operator} {c.Value.ToFullString()}", // compound operators!!!!!
            FunctionDeclarationNode f => $"{(f.IsLocal ? "local " : "")}function {f.Name.ToFullString()}({string.Join(", ", f.Parameters.Select(p => p.ToFullString()).Concat(f.Isvararg ? new[] { "..." } : Array.Empty<string>()))})\n" + FormatBody(f.Body) + "end",  // made it support vararg for the param, and i am now using formatbody on the body
            FunctionCallNode f => $"{f.Function.ToFullString()}({string.Join(", ", f.Arguments.Select(a => a.ToFullString()))})", // function calls
            RepeatNode r => $"repeat\n{FormatBody(r.Body)}until {r.Condition.ToFullString()}",
            ReturnNode r => $"return " + (r.Values.Count > 0 ? string.Join(", ", r.Values.Select(v => v.ToFullString())) : ""),
            NumericForNode fn => $"for {fn.Variable.ToFullString()} = {fn.Start.ToFullString()}, {fn.End.ToFullString()}" + (fn.Step is not null ? $", {fn.Step.ToFullString()}" : "") + $" do\n" + FormatBody(fn.Body) + "end",
            VarargNode _ => "...",
            FunctionExpressionNode f => $"function({string.Join(", ", f.Parameters.Select(p => p.ToFullString()).Concat(f.Isvararg ? new[] { "..." } : Array.Empty<string>()))})\n" + FormatBody(f.Body) + "end",
            _ => throw new Exception("unknown node type")
        };
    }
    private static string FormatBody(List<ExprNode> body) // thank you format body
    {
        return string.Concat(
            body.Select(node =>
                "    " + node.ToFullString().Replace("\n", "\n    ") + "\n"
            )
        );
    }

}