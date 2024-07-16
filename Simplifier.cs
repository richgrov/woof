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
        if (expr.expr is ConstantExpr constant)
        {
            IExpr newExpr = constant.Not();
            LogChange("Apply law of contradiction", expr, newExpr);
            return newExpr;
        }

        if (expr.expr is NotExpr subExpr)
        {
            IExpr newExpr = subExpr.expr;
            LogChange("Apply law of double negation", expr, newExpr);
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
            LogChange("Distribute with De Morgan's law", expr, newExpr);
            return newExpr;
        }

        return expr;
    }

    public IExpr VisitImplication(ImplicationExpr expr)
    {
        IExpr newExpr = new OrExpr(new NotExpr(expr.left), expr.right);
        LogChange("Expand implication", expr, newExpr);
        return newExpr;
    }

    public IExpr VisitDoubleImplication(DoubleImplicationExpr expr)
    {
        IExpr newExpr = new AndExpr(
            new ImplicationExpr(expr.left, expr.right),
            new ImplicationExpr(expr.right, expr.left)
        );
        LogChange("Expand double implication", expr, newExpr);
        return newExpr;
    }

    public IExpr VisitOr(OrExpr expr)
    {
        IExpr? constantFolded = ConstantFoldOr(expr);
        if (constantFolded != null)
        {
            return constantFolded;
        }

        IExpr newLeft = expr.Left.Visit(this);
        IExpr newRight = expr.Right.Visit(this);
        if (!newLeft.Equals(expr.Left) || !newRight.Equals(expr.Right))
        {
            return new OrExpr(newLeft, newRight);
        }

        var expressions = ExpressionsInJunctionChain<OrExpr>(expr);

        var uniqueExpressions = new List<IExpr>();
        foreach (IExpr subExpr in expressions)
        {
            bool hadOpposite = uniqueExpressions.Remove(subExpr.Not());
            if (hadOpposite)
            {
                IExpr simplified = new OrExpr(subExpr, subExpr.Not());
                var constant = new ConstantExpr(true);
                LogChange("Apply law of excluded middle", simplified, constant);
                return constant;
            }

            if (!uniqueExpressions.Contains(subExpr))
            {
                uniqueExpressions.Add(subExpr);
            }
        }

        if (expressions.Count != uniqueExpressions.Count)
        {
            IExpr result = JoinExpressions((e1, e2) => new OrExpr(e1, e2), uniqueExpressions);
            LogChange("Apply law of idempotence", expr, result);
            return result;
        }

        IExpr? distributed = DisjunctionOverConjunction(expr);
        if (distributed != null)
        {
            LogChange("Apply law of disjunction over conjunction", expr, distributed);
            return distributed;
        }

        return expr;
    }

    public IExpr VisitAnd(AndExpr expr)
    {
        IExpr? constantFolded = ConstantFoldAnd(expr);
        if (constantFolded != null)
        {
            return constantFolded;
        }

        var expressions = ExpressionsInJunctionChain<AndExpr>(expr);

        var uniqueExpressions = new List<IExpr>();
        foreach (IExpr subExpr in expressions)
        {
            bool hadOpposite = uniqueExpressions.Remove(subExpr.Not());
            if (hadOpposite)
            {
                IExpr newExpr = new ConstantExpr(false);
                LogChange("Apply law of non-contradiction", expr, newExpr);
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
            LogChange("Apply law of idempotence", expr, result);
            return result;
        }

        return new AndExpr(expr.Left.Visit(this), expr.Right.Visit(this));
    }

    public IExpr VisitConstant(ConstantExpr expr)
    {
        return expr;
    }

    private IExpr? ConstantFoldOr(OrExpr expr)
    {
        IExpr[] left = { expr.Left, expr.Right };
        IExpr[] right = { expr.Right, expr.Left };

        for (int i = 0; i < 2; i++)
        {
            if (!(left[i] is ConstantExpr constant))
            {
                continue;
            }

            if (constant.value)
            {
                LogChange("Apply domination law", expr, constant);
                return constant;
            }
            else
            {
                LogChange("Apply identity law", expr, right[i]);
                return right[i];
            }
        }

        return null;
    }

    private IExpr? ConstantFoldAnd(AndExpr expr)
    {
        IExpr[] left = { expr.Left, expr.Right };
        IExpr[] right = { expr.Right, expr.Left };

        for (int i = 0; i < 2; i++)
        {
            if (!(left[i] is ConstantExpr constant))
            {
                continue;
            }

            if (constant.value)
            {
                LogChange("Apply identity law", expr, right[i]);
                return right[i];
            }
            else
            {
                LogChange("Apply domination law", expr, constant);
                return constant;
            }
        }

        return null;
    }

    public static List<IExpr> ExpressionsInJunctionChain<T>(IExpr expr) where T : IJunctionExpr
    {
        if (!(expr is T junction))
        {
            return new List<IExpr> { expr };
        }

        var expressions = new List<IExpr>();
        expressions.AddRange(ExpressionsInJunctionChain<T>(junction.Left));
        expressions.AddRange(ExpressionsInJunctionChain<T>(junction.Right));
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

    private static IExpr? DisjunctionOverConjunction(OrExpr expr)
    {
        HashSet<string> leftVars = expr.Left.Variables();
        HashSet<string> rightVars = expr.Right.Variables();

        if (leftVars.Intersect(rightVars).Count() > 0)
        {
            if (expr.Left is AndExpr leftAnd)
            {
                return new AndExpr(new OrExpr(leftAnd.Left, expr.Right), new OrExpr(leftAnd.Right, expr.Right));
            }
            else if (expr.Right is AndExpr rightAnd)
            {
                return new AndExpr(new OrExpr(rightAnd.Left, expr.Left), new OrExpr(rightAnd.Right, expr.Left));
            }
        }
        return null;
    }

    public static IExpr SimplifyExpression(IExpr expr)
    {
        return new Simplifier().Simplify(expr);
    }

    private void LogChange(string message, IExpr prev, IExpr now)
    {
        Console.WriteLine($"{Ansi.Yellow}{message}: {Ansi.Reset}{prev} {Ansi.Yellow}becomes {Ansi.Reset}{now}");
    }
}
