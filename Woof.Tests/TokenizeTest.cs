namespace Woof.Tests;

[TestClass]
public class TokenizeTest
{
    [TestMethod]
    public void TestTokenize()
    {
        var input = "(p->q)^(~pvq)<->q";
        var tokens = new List<Token>();

        var tokenizer = new Tokenizer(input);
        while (true)
        {
            Token tok = tokenizer.Next();
            tokens.Add(tok);

            if (tok.type == TokenType.Eof)
            {
                break;
            }
        }

        var expected = new List<Token>{
            new Token(TokenType.OpenParen, 1, 1, null),
            new Token(TokenType.Var, 1, 2, "p"),
            new Token(TokenType.RightArrow, 1, 3, null),
            new Token(TokenType.Var, 1, 5, "q"),
            new Token(TokenType.CloseParen, 1, 6, null),
            new Token(TokenType.UpCaret, 1, 7, null),
            new Token(TokenType.OpenParen, 1, 8, null),
            new Token(TokenType.Not, 1, 9, null),
            new Token(TokenType.Var, 1, 10, "p"),
            new Token(TokenType.DownCaret, 1, 11, null),
            new Token(TokenType.Var, 1, 12, "q"),
            new Token(TokenType.CloseParen, 1, 13, null),
            new Token(TokenType.DoubleArrow, 1, 14, null),
            new Token(TokenType.Var, 1, 17, "q"),
            new Token(TokenType.Eof, 1, 18, null),
        };

        for (int i = 0; i < tokens.Count; i++)
        {
            Assert.AreEqual(expected[i], tokens[i]);
        }
    }
}
