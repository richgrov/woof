namespace Woof;

internal class Parser
{
    private readonly List<Token> _tokens;
    private int _readIndex = 0;

    private Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    IExpr Parse()
    {
        return ParseDisjunction();
    }

    IExpr ParseDisjunction()
    {
        IExpr expr = ParseConjunction();

        while (true)
        {
            Token next = Peek();
            if (next.type == TokenType.DownCaret)
            {
                Consume();
                expr = new OrExpr(expr, ParseConjunction());
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    IExpr ParseConjunction()
    {
        IExpr expr = ParseImplication();

        while (true)
        {
            Token next = Peek();
            if (next.type == TokenType.UpCaret)
            {
                Consume();
                expr = new AndExpr(expr, ParseImplication());
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    IExpr ParseImplication()
    {
        IExpr expr = ParseNot();

        while (true)
        {
            Token next = Peek();
            switch (next.type)
            {
                case TokenType.RightArrow:
                    Consume();
                    expr = new ImplicationExpr(expr, ParseNot());
                    break;

                case TokenType.DoubleArrow:
                    Consume();
                    expr = new DoubleImplicationExpr(expr, ParseNot());
                    break;

                default: goto Done;
            }
            if (next.type == TokenType.RightArrow)
            {
            }
            else
            {
                break;
            }
        }
    Done:

        return expr;
    }

    IExpr ParseNot()
    {
        Token next = Peek();
        if (next.type == TokenType.Not)
        {
            Consume();
            return new NotExpr(ParseNot());
        }

        return ParseGrouping();
    }

    IExpr ParseGrouping()
    {
        Token next = Peek();
        if (next.type == TokenType.OpenParen)
        {
            Consume();

            IExpr expr = Parse();

            Token closing = Consume();
            if (closing.type != TokenType.CloseParen)
            {
                throw new ParseException(closing, $"expected closing parenthesis, got {closing.type}");
            }

            return expr;
        }

        return ParseVariable();
    }

    IExpr ParseVariable()
    {
        Token next = Consume();
        if (next.type == TokenType.Var)
        {
            return new VariableExpr(next.data!.ToString()!);
        }

        throw new ParseException(next, $"unexpected token {next.type}");
    }

    Token Peek()
    {
        return _tokens[Math.Min(_readIndex, _tokens.Count - 1)];
    }

    Token Consume()
    {
        return _tokens[Math.Min(_readIndex++, _tokens.Count - 1)];
    }

    public static IExpr ParseExpr(List<Token> tokens)
    {
        return new Parser(tokens).Parse();
    }
}
