namespace Woof;

internal class Simplifier : Visitor<IExpr>
{
    public void Simplify(IExpr expr)
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

        return new NotExpr(expr.expr.Visit(this));
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

        return new OrExpr(expr.left.Visit(this), expr.right.Visit(this));
    }

    public IExpr VisitAnd(AndExpr expr)
    {
        return new AndExpr(expr.left.Visit(this), expr.right.Visit(this));
    }
}
