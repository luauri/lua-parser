using Parser.AST;
using Xunit;
using Lexer = global::Parser.Lexer;
using LuaParser = global::Parser.Parser;

public class ParserTests
{
    private static List<ExprNode> Parse(string input)
    {
        var tokens = new Lexer().Tokenize(input);
        return new LuaParser().Parse(tokens);
    }

    [Fact]
    public void LocalDeclaration()
    {
        var ast = Parse("local a = 10");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var value = Assert.IsType<ConstantNode>(local.Values[0]);

        Assert.Equal("a", local.Names[0].Name);
        Assert.Equal(10L, value.Value);
        Assert.Equal(Literal.Number, value.Kind);
    }

    [Fact]
    public void StringLiteral()
    {
        var ast = Parse("local a = \"hello world\"");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var value = Assert.IsType<ConstantNode>(local.Values[0]);

        Assert.Equal("hello world", value.Value);
        Assert.Equal(Literal.String, value.Kind);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void BooleanLiteral(string input, bool expected)
    {
        var ast = Parse($"local a = {input}");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var value = Assert.IsType<ConstantNode>(local.Values[0]);

        Assert.Equal(expected, value.Value);
        Assert.Equal(Literal.Boolean, value.Kind);
    }

    [Theory]
    [InlineData("function add() end", false)]
    [InlineData("local function add() end", true)]
    public void FunctionDeclaration(string input, bool isLocal)
    {
        var ast = Parse(input);

        var function = Assert.IsType<FunctionDeclarationNode>(
            Assert.Single(ast)
        );

        Assert.Equal("add", function.Name.Name);
        Assert.Equal(isLocal, function.IsLocal);
        Assert.Empty(function.Parameters);
        Assert.Empty(function.Body);
    }
    [Fact]
    public void RepeatLoop()
    {
        var ast = Parse("""
        repeat
            a = 10
        until a == 10
        """);

        var repeat = Assert.IsType<RepeatNode>(Assert.Single(ast));

        Assert.Single(repeat.Body);
        Assert.IsType<AssignmentNode>(repeat.Body[0]);

        var condition = Assert.IsType<BinaryExpressionNode>(repeat.Condition);

        Assert.Equal("==", condition.Operator);
    }

    [Fact]
    public void RepeatLoopWithMultipleStatements()
    {
        var ast = Parse("""
        repeat
            a = 1
            b = 2
        until a == b
        """);

        var repeat = Assert.IsType<RepeatNode>(Assert.Single(ast));

        Assert.Equal(2, repeat.Body.Count);
        Assert.All(repeat.Body, node => Assert.IsType<AssignmentNode>(node));
    }

    [Fact]
    public void RepeatLoopWithoutUntil()
    {
        Assert.Throws<Exception>(() => Parse("""
        repeat
            a = 10
        """));
    }

    [Fact]
    public void RepeatNestedIf()
    {
        var ast = Parse("""
        repeat
            if a then
                b = 10
            end
        until b == 10
        """);

        var repeat = Assert.IsType<RepeatNode>(Assert.Single(ast));

        Assert.IsType<IfNode>(Assert.Single(repeat.Body));
    }

    [Fact]
    public void FunctionCall()
    {
        var ast = Parse("print(a)");

        var call = Assert.IsType<FunctionCallNode>(Assert.Single(ast));

        Assert.Equal("print", Assert.IsType<IdentifierNode>(call.Function).Name);
        Assert.Equal("a", Assert.IsType<IdentifierNode>(Assert.Single(call.Arguments)).Name);
    }

    [Fact]
    public void FunctionCallWithArguments()
    {
        var ast = Parse("local result = add(10, 20)");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var call = Assert.IsType<FunctionCallNode>(Assert.Single(local.Values));

        Assert.Equal("add", Assert.IsType<IdentifierNode>(call.Function).Name);
        Assert.Equal(2, call.Arguments.Count);
        Assert.Equal(10L, Assert.IsType<ConstantNode>(call.Arguments[0]).Value);
        Assert.Equal(20L, Assert.IsType<ConstantNode>(call.Arguments[1]).Value);
    }

    [Fact]
    public void EmptyFunctionCall()
    {
        var ast = Parse("print()");

        var call = Assert.IsType<FunctionCallNode>(Assert.Single(ast));

        Assert.Empty(call.Arguments);
    }

    [Fact]
    public void FunctionCallInsideIf()
    {
        var ast = Parse("""
        if a then
            print(a)
        end
        """);

        var ifNode = Assert.IsType<IfNode>(Assert.Single(ast));

        var call = Assert.IsType<FunctionCallNode>(
            Assert.Single(ifNode.Body)
        );

        Assert.Equal("print", Assert.IsType<IdentifierNode>(call.Function).Name);
    }

    [Fact]
    public void NestedFunctionCall()
    {
        var ast = Parse("print(add(1, 2))");

        var outer = Assert.IsType<FunctionCallNode>(Assert.Single(ast));
        var inner = Assert.IsType<FunctionCallNode>(Assert.Single(outer.Arguments));

        Assert.Equal("print", Assert.IsType<IdentifierNode>(outer.Function).Name);
        Assert.Equal("add", Assert.IsType<IdentifierNode>(inner.Function).Name);
        Assert.Equal(2, inner.Arguments.Count);
    }

    [Fact]
    public void FunctionParameters()
    {
        var ast = Parse("function add(a, b) end");

        var function = Assert.IsType<FunctionDeclarationNode>(
            Assert.Single(ast)
        );

        Assert.Equal(2, function.Parameters.Count);
        Assert.Equal("a", function.Parameters[0].Name);
        Assert.Equal("b", function.Parameters[1].Name);
    }

    [Fact]
    public void VariableReference()
    {
        var ast = Parse("local b = a");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var value = Assert.IsType<IdentifierNode>(local.Values[0]);

        Assert.Equal("b", local.Names[0].Name);
        Assert.Equal("a", value.Name);
    }

    [Fact]
    public void MultipleDeclarations()
    {
        var ast = Parse("local a = 10\nlocal b = a");

        Assert.Equal(2, ast.Count);

        var first = Assert.IsType<LocalDeclaration>(ast[0]);
        var second = Assert.IsType<LocalDeclaration>(ast[1]);

        Assert.Equal("a", first.Names[0].Name);
        Assert.Equal("b", second.Names[0].Name);
        Assert.IsType<IdentifierNode>(second.Values[0]);
    }

    [Fact]
    public void MultipleLocalVariables()
    {
        var ast = Parse("local a, b, c = 10, 20, 30");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));

        Assert.Equal(3, local.Names.Count);
        Assert.Equal(3, local.Values.Count);

        Assert.Equal("a", local.Names[0].Name);
        Assert.Equal("b", local.Names[1].Name);
        Assert.Equal("c", local.Names[2].Name);

        Assert.Equal(10L, Assert.IsType<ConstantNode>(local.Values[0]).Value);
        Assert.Equal(20L, Assert.IsType<ConstantNode>(local.Values[1]).Value);
        Assert.Equal(30L, Assert.IsType<ConstantNode>(local.Values[2]).Value);
    }

    [Fact]
    public void MultipleLocalVariableReferences()
    {
        var ast = Parse("local a, b = 10, a");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));

        Assert.Equal(2, local.Names.Count);
        Assert.Equal(2, local.Values.Count);

        Assert.Equal(10L, Assert.IsType<ConstantNode>(local.Values[0]).Value);
        Assert.Equal("a", Assert.IsType<IdentifierNode>(local.Values[1]).Name);
    }

    [Fact]
    public void NilLiteral()
    {
        var ast = Parse("local a = nil");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var value = Assert.IsType<ConstantNode>(local.Values[0]);

        Assert.Null(value.Value);
        Assert.Equal(Literal.Nil, value.Kind);
    }

    [Fact]
    public void DecimalLiteral()
    {
        var ast = Parse("local a = 10.5");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var value = Assert.IsType<ConstantNode>(local.Values[0]);

        Assert.Equal(10.5, value.Value);
        Assert.Equal(Literal.Number, value.Kind);
    }

    [Theory]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("*")]
    [InlineData("/")]
    public void MathExpression(string op)
    {
        var ast = Parse($"local a = 10 {op} 5");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var expression = Assert.IsType<BinaryExpressionNode>(local.Values[0]);

        Assert.Equal(op, expression.Operator);

        var left = Assert.IsType<ConstantNode>(expression.Left);
        var right = Assert.IsType<ConstantNode>(expression.Right);

        Assert.Equal(10L, left.Value);
        Assert.Equal(5L, right.Value);
    }

    [Fact]
    public void MathExpressionWithVariable()
    {
        var ast = Parse("local a = b + 10");

        var local = Assert.IsType<LocalDeclaration>(Assert.Single(ast));
        var expression = Assert.IsType<BinaryExpressionNode>(local.Values[0]);

        Assert.Equal("+", expression.Operator);
        Assert.Equal("b", Assert.IsType<IdentifierNode>(expression.Left).Name);
        Assert.Equal(10L, Assert.IsType<ConstantNode>(expression.Right).Value);
    }

    [Fact]
    public void VariableAssignment()
    {
        var ast = Parse("a = 20");

        var assignment = Assert.IsType<AssignmentNode>(Assert.Single(ast));
        var value = Assert.IsType<ConstantNode>(assignment.Value);

        Assert.Equal("a", assignment.Name.Name);
        Assert.Equal(20L, value.Value);
    }

    [Fact]
    public void VariableReassignment()
    {
        var ast = Parse("local a = 10\na = 20");

        Assert.Equal(2, ast.Count);

        Assert.IsType<LocalDeclaration>(ast[0]);

        var assignment = Assert.IsType<AssignmentNode>(ast[1]);

        Assert.Equal("a", assignment.Name.Name);
        Assert.Equal(20L, Assert.IsType<ConstantNode>(assignment.Value).Value);
    }

    [Theory]
    [InlineData("+=")]
    [InlineData("-=")]
    [InlineData("*=")]
    [InlineData("/=")]
    public void CompoundAssignment(string op)
    {
        var ast = Parse($"a {op} 5");

        var assignment = Assert.IsType<CompoundAssignmentNode>(Assert.Single(ast));

        Assert.Equal("a", assignment.Name.Name);
        Assert.Equal(op, assignment.Operator);
        Assert.Equal(5L, Assert.IsType<ConstantNode>(assignment.Value).Value);
    }

    [Fact]
    public void InvalidSyntax()
    {
        Assert.Throws<Exception>(() => Parse("local = 10"));
    }

    [Fact]
    public void CompoundAssignmentCannotBeUsedAsAnExpression()
    {
        Assert.Throws<Exception>(() => Parse("local a = 1 += 1"));
    }

    [Fact]
    public void IncompleteDeclaration()
    {
        Assert.Throws<Exception>(() => Parse("local a ="));
    }
}
