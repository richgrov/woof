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
    public IExpr Left { get; }
    public IExpr Right { get; }
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

internal class OrExpr : IExpr, IJunctionExpr
{
    public IExpr Left { get; private set; }
    public IExpr Right { get; private set; }

    public OrExpr(IExpr left, IExpr right)
    {
        Left = left;
        Right = right;
    }

    public bool IsConjunctive => false;

    public T Visit<T>(Visitor<T> v) => v.VisitOr(this);

    public HashSet<string> Variables() => Left.Variables().Union(Right.Variables()).ToHashSet();

    public IExpr Not()
    {
        return new AndExpr(Left.Not(), Right.Not());
    }

    public override bool Equals(object? obj)
    {
        if (obj is OrExpr expr)
        {
            return (expr.Left.Equals(Left) && expr.Right.Equals(Right)) ||
                (expr.Left.Equals(Right) && expr.Right.Equals(Left));
        }
        return false;
    }

    public override int GetHashCode() => ToString().GetHashCode();

    public override string ToString()
    {
        return $"({Left} v {Right})";
    }
}

internal record AndExpr : IExpr, IJunctionExpr
{
    public IExpr Left { get; private set; }
    public IExpr Right { get; private set; }

    public AndExpr(IExpr left, IExpr right)
    {
        Left = left;
        Right = right;
    }

    public bool IsConjunctive => true;

    public T Visit<T>(Visitor<T> v) => v.VisitAnd(this);

    public HashSet<string> Variables() => Left.Variables().Union(Right.Variables()).ToHashSet();

    public IExpr Not()
    {
        return new OrExpr(Left.Not(), Right.Not());
    }

    public override string ToString()
    {
        return $"({Left} ^ {Right})";
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
