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
        if (expr.left.ToString() == expr.right.ToString())
        {
            Console.WriteLine($"Apply law of idempotence: {expr} -> {expr.left}");
            return expr.left;
        }

        if (expr.left.Not().ToString() == expr.right.ToString())
        {
            Console.WriteLine($"Apply law of excluded middle: {expr} -> t");
            return new ConstantExpr(true);
        }

        return new OrExpr(expr.left.Visit(this), expr.right.Visit(this));
    }

    public IExpr VisitAnd(AndExpr expr)
    {
        if (expr.left.Not().ToString() == expr.right.ToString())
        {
            IExpr newExpr = new ConstantExpr(false);
            Console.WriteLine($"Apply law of non-contradiction: {expr} -> {newExpr}");
            return newExpr;
        }

        return new AndExpr(expr.left.Visit(this), expr.right.Visit(this));
    }

    public IExpr VisitConstant(ConstantExpr expr)
    {
        return expr;
    }

    public static IExpr SimplifyExpression(IExpr expr)
    {
        return new Simplifier().Simplify(expr);
    }
}
