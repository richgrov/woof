namespace Woof;

internal class Simplifier : Visitor<IExpr>
{
    public void Simplify(IExpr expr)
    {
        expr.Visit(this);
    }

    public IExpr VisitVariable(VariableExpr expr)
    {
        return expr;
    }

    public IExpr VisitNot(NotExpr expr)
    {
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
        return expr;
    }

    public IExpr VisitAnd(AndExpr expr)
    {
        return expr;
    }
}
