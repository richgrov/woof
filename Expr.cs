namespace Woof;

internal interface Visitor<T>
{
    T VisitVariable(VariableExpr expr);
    T VisitNot(NotExpr expr);
    T VisitImplication(ImplicationExpr expr);
    T VisitOr(OrExpr expr);
    T VisitAnd(AndExpr expr);
}

internal interface IExpr
{
    public T Visit<T>(Visitor<T> visitor);
}

internal record VariableExpr(char c) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitVariable(this);

    public override string ToString()
    {
        return c.ToString();
    }
}

internal record NotExpr(IExpr expr) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitNot(this);

    public override string ToString()
    {
        return "~" + expr.ToString();
    }
}

internal record ImplicationExpr(IExpr left, IExpr right) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitImplication(this);

    public override string ToString()
    {
        return $"({left}) v ({right})";
    }
}

internal record OrExpr(IExpr left, IExpr right) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitOr(this);

    public override string ToString()
    {
        return $"({left}) v ({right})";
    }
}

internal record AndExpr(IExpr left, IExpr right) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitAnd(this);

    public override string ToString()
    {
        return $"({left}) ^ ({right})";
    }
}
