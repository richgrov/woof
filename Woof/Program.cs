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
                tokens.Add(tok);

                if (tok.type == TokenType.Eof)
                {
                    break;
                }
            }

            IExpr expr = Parser.ParseExpr(tokens);
            Console.WriteLine(Ansi.Up + expr + "\n");
            Evaluator.PrintTruthTable(expr);
            Console.WriteLine();

            IExpr simplified = Simplifier.SimplifyExpression(expr);
            if (simplified.ToString() != expr.ToString())
            {
                Console.WriteLine("Simplified: " + simplified + "\n");
                Evaluator.PrintTruthTable(simplified);
            }
        }
    }
}
