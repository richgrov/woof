namespace Woof;

public class ParseException : Exception
{
    public ParseException(int line, int col, string message)
        : base($"{line}:{col}: {message}")
    { }

    public ParseException(Token token, string message)
        : this(token.line, token.col, message)
    { }
}
