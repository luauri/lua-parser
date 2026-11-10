using System;
using Parser;

namespace Parser;

public partial class Lexer
{
    public enum TokenType
    {
        Keyword,
        Identifier,
        Equals,
        Number,
        String,
        Boolean,
        Nil,
        Comma,
        Operator,
        Rparen,
        Lparen,
        Vararg,
        // Semicolon
    }

    public record Token(TokenType Type, string Value);

    private static bool Match(string input, int index, string text) //  🤤 
    {
        return input.AsSpan(index).StartsWith(text, StringComparison.Ordinal);
    }

    public List<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();

        int i = 0;
        while (i < input.Length)
        {
            char c = input[i];

            if (char.IsWhiteSpace(c))
            {
                i++; // skip whitespace
                continue;
            }
            if (Match(input, i, "--")) // TODO: make this more lossless
            {
                Comment(input, ref i);
                continue;
            }

            if (c == '=')
            {
                if (Match(input, i, "=="))
                {
                    tokens.Add(new Token(TokenType.Operator, "=="));
                    i += 2;
                    continue;
                }
                tokens.Add(new Token(TokenType.Equals, "="));
                i++;
                continue;
            }
            if (char.IsLetter(c) || c == '_')                 // supported variables starting with _
            {
                tokens.Add(Identifier(input, ref i));
                continue;
            }

            if (char.IsDigit(c))
            {
                tokens.Add(Number(input, ref i));
                continue;
            }
            if (c == '"' || c == '\'')
            {
                tokens.Add(String(input, ref i));
                continue;
            }
            if (c == '(')
            {
                tokens.Add(new Token(TokenType.Lparen, "("));
                i++;
                continue;
            }
            if (c == ')')
            {
                tokens.Add(new Token(TokenType.Rparen, ")"));
                i++;
                continue;
            }
            if (c == ';')
            {
                i++;
                continue;
            }

            if (c == ',')
            {
                tokens.Add(new Token(TokenType.Comma, ",")); 
                i++;
                continue;
            }
            if (c == '>')
            {
                if (Match(input, i, ">="))  // check if it's >= instead of >
                {
                    tokens.Add(new Token(TokenType.Operator, ">="));
                    i += 2;
                    continue;
                }
                tokens.Add(new Token(TokenType.Operator, ">"));
                i++;
                continue;
            }
            if (c == '<')
            {
                if (Match(input, i, "<="))
                {
                    tokens.Add(new Token(TokenType.Operator, "<="));
                    i += 2;
                    continue;
                }
                tokens.Add(new Token(TokenType.Operator, "<"));
                i++;
                continue;
            }

            if (c is '+' or '-' or '*' or '/') 
            {
                Operators(input, ref i, tokens);
                continue;
            }
            if (Match(input, i, "..."))
            {
                tokens.Add(new Token(TokenType.Vararg, "..."));
                i += 3;
                continue;
            }
            throw new Exception($"unexpected character: {c}");
        }
        return tokens;
    }
}
