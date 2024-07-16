namespace Woof;

internal class Evaluator : Visitor<bool>
{
    private Dictionary<string, bool> _vars;

    public Evaluator(Dictionary<string, bool> vars)
    {
        _vars = vars;
    }

    public bool Eval(IExpr expr) => expr.Visit(this);

    public bool GetVariable(string id)
    {
        return _vars[id];
    }

    public bool VisitVariable(VariableExpr expr)
    {
        return _vars[expr.c.ToString()];
    }

    bool Visitor<bool>.VisitNot(NotExpr expr)
    {
        return !expr.expr.Visit(this);
    }

    bool Visitor<bool>.VisitImplication(ImplicationExpr expr)
    {
        return !expr.left.Visit(this) || expr.right.Visit(this);
    }

    bool Visitor<bool>.VisitOr(OrExpr expr)
    {
        return expr.left.Visit(this) || expr.right.Visit(this);
    }

    bool Visitor<bool>.VisitAnd(AndExpr expr)
    {
        return expr.left.Visit(this) && expr.right.Visit(this);
    }
}
