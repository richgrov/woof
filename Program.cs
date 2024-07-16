namespace Woof;

internal class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            string input = Console.ReadLine()!;

            var tokenizer = new Tokenizer(input);
            var tokens = new List<Token>();

            while (true)
            {
                Token tok = tokenizer.Next();
                Console.WriteLine(tok);
                tokens.Add(tok);

                if (tok.type == TokenType.Eof)
                {
                    break;
                }
            }

            IExpr expr = Parser.ParseExpr(tokens);
            Console.WriteLine(expr);

            new Evaluator("p", "q").Eval(expr);
            new Simplifier().Simplify(expr);
        }
    }
}
