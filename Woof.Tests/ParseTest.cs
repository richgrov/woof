namespace Woof.Tests;

[TestClass]
public class ParseTest
{
    [TestMethod]
    public void TestVariable()
    {
        Assert.AreEqual(new VariableExpr("p"), ToExpr("p"));
    }

    [TestMethod]
    public void TestNot()
    {
        Assert.AreEqual(new NotExpr(new VariableExpr("p")), ToExpr("~p"));
    }

    [TestMethod]
    public void TestImplication()
    {
        Assert.AreEqual(new ImplicationExpr(new VariableExpr("p"), new VariableExpr("q")), ToExpr("p->q"));
    }

    [TestMethod]
    public void TestDoubleImplication()
    {
        Assert.AreEqual(new DoubleImplicationExpr(new VariableExpr("p"), new VariableExpr("q")), ToExpr("p<->q"));
    }

    [TestMethod]
    public void TestOr()
    {
        Assert.AreEqual(new OrExpr(new VariableExpr("p"), new VariableExpr("q")), ToExpr("pvq"));
    }

    [TestMethod]
    public void TestAnd()
    {
        Assert.AreEqual(new AndExpr(new VariableExpr("p"), new VariableExpr("q")), ToExpr("p^q"));
    }

    [TestMethod]
    public void TestIncompleteParenthesisThrows()
    {
        var tokens = ToTokens("(p->q");
        Assert.ThrowsException<ParseException>(() => Parser.ParseExpr(tokens));
    }

    private static List<Token> ToTokens(string input)
    {
        var tokens = new List<Token>();
        var tokenizer = new Tokenizer(input);
        while (true)
        {
            var tok = tokenizer.Next();
            tokens.Add(tok);
            if (tok.type == TokenType.Eof)
            {
                break;
            }
        }

        return tokens;
    }

    private static IExpr ToExpr(string input)
    {
        return Parser.ParseExpr(ToTokens(input));
    }
}
