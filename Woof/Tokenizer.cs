namespace Woof;

public class Tokenizer
{
    private readonly string _input;
    private int _readIndex = 0;
    private int _line = 1;
    private int _col = 1;

    public Tokenizer(string input)
    {
        _input = input;
    }

    public Token Next()
    {
        ConsumeWhitespace();

        if (_readIndex >= _input.Length)
        {
            return new Token(TokenType.Eof, _line, _col, null);
        }

        char c = _input[_readIndex++];
        int col = _col++;
        switch (c)
        {
            case '^':
                return new Token(TokenType.UpCaret, _line, col, null);
            case 'v':
                return new Token(TokenType.DownCaret, _line, col, null);
            case '(':
                return new Token(TokenType.OpenParen, _line, col, null);
            case ')':
                return new Token(TokenType.CloseParen, _line, col, null);
            case '~':
                return new Token(TokenType.Not, _line, col, null);
            case '-':
                char after = _input[_readIndex++];
                _col++;
                if (after == '>')
                {
                    return new Token(TokenType.RightArrow, _line, col, null);
                }
                else
                {
                    throw new ParseException(_line, col, $"expected '>' after hyphen, got {after}");
                }
            case '<':
                char after1 = _input[_readIndex++];
                char after2 = _input[_readIndex++];
                _col += 2;
                if (after1 == '-' && after2 == '>')
                {
                    return new Token(TokenType.DoubleArrow, _line, col, null);
                }
                break;

            default:
                if (char.IsLetter(c))
                {
                    return new Token(TokenType.Var, _line, col, c.ToString());
                }
                break;
        }

        throw new ParseException(_line, col, $"invalid character {c}");
    }

    private void ConsumeWhitespace()
    {
        while (_readIndex < _input.Length)
        {
            char next = _input[_readIndex];
            if (next == '\n' || next == '\r')
            {
                _line++;
                _col = 1;
            }
            else if (next == ' ')
            {
                _col++;
            }
            else
            {
                return;
            }

            _readIndex++;
        }
    }
}

public record Token(TokenType type, int line, int col, object? data);

public enum TokenType
{
    Eof,
    Var,
    UpCaret,
    DownCaret,
    OpenParen,
    CloseParen,
    Not,
    RightArrow,
    DoubleArrow,
}
