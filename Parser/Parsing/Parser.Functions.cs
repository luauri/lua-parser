using Parser.AST;

using TokenType = global::Parser.Lexer.TokenType;
namespace Parser;

public partial class Parser
{
    private ExprNode ParseFunctionExpression()
    {
        Expect(TokenType.Lparen); // function(

        List<IdentifierNode> parameters = new();
        bool isVararg = false;

        if (Peek()?.Type != TokenType.Rparen)
        {
            while (true)
            {
                if (Match(TokenType.Vararg))
                {
                    isVararg = true;
                    break;
                }

                parameters.Add(ParseIdentifier());
                if (!Match(TokenType.Comma))
                    break;
            }
        }

        Expect(TokenType.Rparen); // function ()

        List<ExprNode> body = new();
        while (Peek() is not null && Peek()!.Value != "end")
        {
            body.Add(ParseStatement());
        }

        ExpectKeyword("end");
        return new FunctionExpressionNode(parameters, body, isVararg);
    }
    private ExprNode ParseFunction(bool isLocal)
    {
        IdentifierNode name = ParseIdentifier(); // function name

        Expect(TokenType.Lparen);

        List<IdentifierNode> parameters = new();
        bool Isvararg = false;

        if (Peek()?.Type != TokenType.Rparen)
        {
            while (true)
            {
                if (Match(TokenType.Vararg))
                {
                    Isvararg = true;
                    break;
                }

                parameters.Add(ParseIdentifier());
                if (!Match(TokenType.Comma))
                    break;
            }
        }
        Expect(TokenType.Rparen); // closing paren
        List<ExprNode> body = new(); // function body

        while (Peek() is not null && Peek()!.Value != "end")
        {
            body.Add(ParseStatement());
        }

        if (Peek()?.Value != "end")
            throw new Exception("Expected 'end' after function");

        Advance();
        return new FunctionDeclarationNode(name, parameters, body, isLocal, Isvararg);
    }
}