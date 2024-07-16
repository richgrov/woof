namespace Woof;

internal interface Visitor<T>
{
    T VisitVariable(VariableExpr expr);
    T VisitNot(NotExpr expr);
    T VisitImplication(ImplicationExpr expr);
    T VisitDoubleImplication(DoubleImplicationExpr expr);
    T VisitOr(OrExpr expr);
    T VisitAnd(AndExpr expr);
    T VisitConstant(ConstantExpr expr);
}

internal interface IExpr
{
    public T Visit<T>(Visitor<T> visitor);

    public IExpr Not();

    public HashSet<string> Variables();

    public bool Equals(object? obj)
    {
        return obj is IExpr && obj.ToString() == ToString();
    }

    public int GetHashCode()
    {
        return ToString()!.GetHashCode();
    }
}

internal interface IJunctionExpr : IExpr
{
    public IExpr left { get; }
    public IExpr right { get; }
    public bool IsConjunctive { get; }
}

internal record VariableExpr(string id) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitVariable(this);

    public HashSet<string> Variables() => new HashSet<string> { id };

    public IExpr Not()
    {
        return new NotExpr(this);
    }

    public override string ToString()
    {
        return id.ToString();
    }
}

internal record NotExpr(IExpr expr) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitNot(this);

    public HashSet<string> Variables() => expr.Variables();

    public IExpr Not()
    {
        return expr;
    }

    public override string ToString()
    {
        return "~" + expr.ToString();
    }
}

internal record ImplicationExpr(IExpr left, IExpr right) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitImplication(this);

    public HashSet<string> Variables() => left.Variables().Union(right.Variables()).ToHashSet();

    public IExpr Not()
    {
        return new AndExpr(left.Not(), right.Not());
    }

    public override string ToString()
    {
        return $"({left} -> {right})";
    }
}

internal record DoubleImplicationExpr(IExpr left, IExpr right) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitDoubleImplication(this);

    public HashSet<string> Variables() => left.Variables().Union(right.Variables()).ToHashSet();

    public IExpr Not()
    {
        return new OrExpr(new ImplicationExpr(left, right).Not(), new ImplicationExpr(right, left).Not());
    }

    public override string ToString()
    {
        return $"({left} <-> {right})";
    }
}

internal record OrExpr(IExpr left, IExpr right) : IExpr, IJunctionExpr
{
    public bool IsConjunctive => false;

    public T Visit<T>(Visitor<T> v) => v.VisitOr(this);

    public HashSet<string> Variables() => left.Variables().Union(right.Variables()).ToHashSet();

    public IExpr Not()
    {
        return new AndExpr(left.Not(), right.Not());
    }

    public override string ToString()
    {
        return $"({left} v {right})";
    }
}

internal record AndExpr(IExpr left, IExpr right) : IExpr, IJunctionExpr
{
    public bool IsConjunctive => true;

    public T Visit<T>(Visitor<T> v) => v.VisitAnd(this);

    public HashSet<string> Variables() => left.Variables().Union(right.Variables()).ToHashSet();

    public IExpr Not()
    {
        return new OrExpr(left.Not(), right.Not());
    }

    public override string ToString()
    {
        return $"({left} ^ {right})";
    }
}

internal record ConstantExpr(bool value) : IExpr
{
    public T Visit<T>(Visitor<T> v) => v.VisitConstant(this);

    public HashSet<string> Variables() => new();

    public IExpr Not()
    {
        return new ConstantExpr(!value);
    }

    public override string ToString()
    {
        return value ? "t" : "~t";
    }
}
