namespace Woof;

internal interface Visitor<T>
{
    T VisitVariable(VariableExpr expr);
    T VisitNot(NotExpr expr);
    T VisitImplication(ImplicationExpr expr);
    T VisitOr(OrExpr expr);
    T VisitAnd(AndExpr expr);
    T VisitConstant(ConstantExpr expr);
}

internal interface IExpr
{
    public T Visit<T>(Visitor<T> visitor);
}

internal record VariableExpr(string id) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitVariable(this);

    public override string ToString()
    {
        return id.ToString();
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

internal record ConstantExpr(bool value) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitConstant(this);

    public IExpr Not()
    {
        return new ConstantExpr(!value);
    }

    public override string ToString()
    {
        return value ? "t" : "~t";
    }
}
