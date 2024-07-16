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

            string[] vars = { "p", "q" };
            EvaluateAll(vars, 0, new bool[] { false, false }, expr);
        }
    }

    public static void EvaluateAll(string[] vars, int index, bool[] values, IExpr expr)
    {
        if (index >= vars.Length)
        {
            var varDict = new Dictionary<string, bool>();
            for (int i = 0; i < vars.Length; i++)
            {
                varDict[vars[i]] = values[i];
            }

            var eval = new Evaluator(varDict);
            Console.WriteLine(eval.Eval(expr));
            return;
        }

        values[index] = true;
        EvaluateAll(vars, index + 1, values, expr);
        values[index] = false;
        EvaluateAll(vars, index + 1, values, expr);
    }
}
