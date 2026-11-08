using Parser.AST;
using System;
namespace Parser;

using Token = global::Parser.Lexer.Token;
public partial class Parser
{
    private ExprNode ParseStatement()
    {
        Token token = Peek()!; // using new helper now
        if (token.Type == Lexer.TokenType.Keyword)
        {
            if (token.Value == "if")
            {
                Advance();
                return ParseIf();
            }
            if (token.Value == "while")
            {
                Advance();
                return ParseWhile();
            }
            if (token.Value == "repeat")
            {
                Advance(); 
                return ParseRepeat();
            }
            if (token.Value == "local")
            {
                Advance();

                if (Peek()?.Value == "function")
                {
                    Advance();
                    return ParseFunction(true); // omg.
                }

                pos--;

                return ParseVariable();
            }

            if (token.Value == "function")
            {
                Advance();
                return ParseFunction(false);
            }

        }
        if (token.Type == Lexer.TokenType.Identifier) // accidentally had this in the if block that checked if it was a keyword, not a identifier...
        {
            if (pos + 1 < tokens.Count && tokens[pos + 1].Type == Lexer.TokenType.Lparen)// check if it's a function call instead of a assignment 
            {
                ExprNode expression = ParseExpression();
                if (expression is not FunctionCallNode)
                    throw new Exception("expected a function call");
                return expression;
            }
            return ParseAssignment();
        }

        throw new Exception($"unexpected token: {token.Value}");
    }

    private ExprNode ParseVariable()
    {
        Advance();

        if (pos >= tokens.Count)
            throw new Exception("Incomplete variable declaration");

        List<IdentifierNode> names = ParseList(ParseIdentifier);
        List<ExprNode> values = new List<ExprNode>();

        // if (pos >= tokens.Count)
        //     throw new Exception("Incomplete variable declaration");

        // Expect(Lexer.TokenType.Equals, "Expected '='");

        if (Match(Lexer.TokenType.Equals))
        {
            if (pos >= tokens.Count)
                throw new Exception("Incomplete variable declaration");
            values = ParseList(ParseExpression);
        }

        // values = ParseList(ParseExpression);
        return new LocalDeclaration(names, values);
    }
    private ExprNode ParseAssignment()
    {
        Token name = Expect(Lexer.TokenType.Identifier, "Expected identifier");

        if (Match(Lexer.TokenType.Equals))
        {
            ExprNode value = ParseExpression();

            return new AssignmentNode(
                new IdentifierNode(name.Value),
                value
            );
        }

        if (Peek()?.Type == Lexer.TokenType.Operator &&
            Peek()!.Value is "+=" or "-=" or "*=" or "/=")
        {
            Token op = Advance();
            ExprNode value = ParseExpression();

            return new CompoundAssignmentNode(
                new IdentifierNode(name.Value),
                op.Value,
                value
            );
        }

        throw new Exception("Expected '='");
    }
}