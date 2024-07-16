namespace Woof;

internal class Simplifier : Visitor<IExpr>
{
    private IExpr Simplify(IExpr expr)
    {
        IExpr runningExpr = expr;
        while (true)
        {
            IExpr newExpr = runningExpr.Visit(this);
            if (newExpr.ToString() == runningExpr.ToString())
            {
                break;
            }

            runningExpr = newExpr;
        }

        return runningExpr;
    }

    public IExpr VisitVariable(VariableExpr expr)
    {
        return expr;
    }

    public IExpr VisitNot(NotExpr expr)
    {
        if (expr.expr is NotExpr subExpr)
        {
            IExpr newExpr = subExpr.expr;
            Console.WriteLine($"Apply law of double negation: {expr} -> {newExpr}");
            return newExpr;
        }

        IExpr simplifiedInner = expr.expr.Visit(this);
        if (simplifiedInner.ToString() != expr.expr.ToString())
        {
            return simplifiedInner;
        }

        bool isBinaryInner = expr.expr is AndExpr || expr.expr is OrExpr || expr.expr is ImplicationExpr;
        if (isBinaryInner)
        {
            IExpr newExpr = expr.expr.Not();
            Console.WriteLine($"Distribute with De Morgan's law: {expr} -> {newExpr}");
            return newExpr;
        }

        return expr;
    }

    public IExpr VisitImplication(ImplicationExpr expr)
    {
        IExpr newExpr = new AndExpr(new NotExpr(expr.left), expr.right);
        Console.WriteLine($"Expand implication: {expr} -> {newExpr}");
        return newExpr;
    }

    public IExpr VisitOr(OrExpr expr)
    {
        var expressions = ExpressionsInJunctionChain<OrExpr>(expr);

        var uniqueExpressions = new List<IExpr>();
        foreach (IExpr subExpr in expressions)
        {
            bool hadOpposite = uniqueExpressions.Remove(subExpr.Not());
            if (hadOpposite)
            {
                Console.WriteLine($"Apply law of excluded middle: {expr} -> t");
                return new ConstantExpr(true);
            }

            if (!uniqueExpressions.Contains(subExpr))
            {
                uniqueExpressions.Add(subExpr);
            }
        }

        if (expressions.Count != uniqueExpressions.Count)
        {
            IExpr result = JoinExpressions((e1, e2) => new OrExpr(e1, e2), uniqueExpressions);
            Console.WriteLine($"Apply law of idempotence: {expr} -> {result}");
            return result;
        }

        return new OrExpr(expr.left.Visit(this), expr.right.Visit(this));
    }

    public IExpr VisitAnd(AndExpr expr)
    {
        var expressions = ExpressionsInJunctionChain<AndExpr>(expr);

        var uniqueExpressions = new List<IExpr>();
        foreach (IExpr subExpr in expressions)
        {
            bool hadOpposite = uniqueExpressions.Remove(subExpr.Not());
            if (hadOpposite)
            {
                IExpr newExpr = new ConstantExpr(false);
                Console.WriteLine($"Apply law of non-contradiction: {expr} -> {newExpr}");
                return newExpr;
            }

            if (!uniqueExpressions.Contains(subExpr))
            {
                uniqueExpressions.Add(subExpr);
            }
        }

        if (expressions.Count != uniqueExpressions.Count)
        {
            IExpr result = JoinExpressions((e1, e2) => new AndExpr(e1, e2), uniqueExpressions);
            Console.WriteLine($"Apply law of idempotence: {expr} -> {result}");
            return result;
        }

        return new AndExpr(expr.left.Visit(this), expr.right.Visit(this));
    }

    public IExpr VisitConstant(ConstantExpr expr)
    {
        return expr;
    }

    public static List<IExpr> ExpressionsInJunctionChain<T>(IExpr expr) where T : IJunctionExpr
    {
        if (!(expr is T junction))
        {
            return new List<IExpr> { expr };
        }

        var expressions = new List<IExpr>();
        expressions.AddRange(ExpressionsInJunctionChain<T>(junction.left));
        expressions.AddRange(ExpressionsInJunctionChain<T>(junction.right));
        return expressions;
    }

    public static IExpr JoinExpressions(Func<IExpr, IExpr, IExpr> combiner, List<IExpr> expressions)
    {
        IExpr result = expressions[0];
        for (int i = 1; i < expressions.Count; i++)
        {
            result = combiner(result, expressions[i]);
        }
        return result;
    }

    public static IExpr SimplifyExpression(IExpr expr)
    {
        return new Simplifier().Simplify(expr);
    }
}
