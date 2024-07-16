namespace Woof;

internal class Evaluator : Visitor<bool>
{
    private string[] _vars;
    private Dictionary<string, bool> _values = new();
    private bool _printingHeader = false;

    private HashSet<string> _seenColunms = new();

    private Evaluator(params string[] vars)
    {
        _vars = vars;
        foreach (string var in _vars)
        {
            _values[var] = false;
        }
    }

    private void Eval(IExpr expr)
    {
        PrintRow(expr, header: true);
        EvalCombinations(0, expr);
    }

    private void EvalCombinations(int varIndex, IExpr expr)
    {
        if (varIndex >= _vars.Length)
        {
            PrintRow(expr, header: false);
            return;
        }

        string variable = _vars[varIndex];
        _values[variable] = true;
        EvalCombinations(varIndex + 1, expr);
        _values[variable] = false;
        EvalCombinations(varIndex + 1, expr);
    }

    private void PrintRow(IExpr expr, bool header)
    {
        _printingHeader = header;
        _seenColunms.Clear();

        foreach (string id in _vars)
        {
            PrintCell(new VariableExpr(id), _values[id]);
        }

        expr.Visit(this);
        Console.WriteLine();
    }

    private void PrintCell(IExpr expr, bool value)
    {
        if (expr is ConstantExpr)
        {
            return;
        }

        string exprString = expr.ToString()!;
        if (!_seenColunms.Add(exprString))
        {
            return;
        }

        if (_seenColunms.Count > 1)
        {
            Console.Write(" | ");
        }

        if (_printingHeader)
        {
            Console.Write(exprString);
        }
        else
        {
            Console.Write(value ? "T" : "F");
            Console.Write(new string(' ', exprString.Length - 1));
        }
    }

    public bool VisitVariable(VariableExpr expr)
    {
        bool result = _values[expr.id];
        PrintCell(expr, result);
        return result;
    }

    public bool VisitNot(NotExpr expr)
    {
        bool result = !expr.expr.Visit(this);
        PrintCell(expr, result);
        return result;
    }

    public bool VisitImplication(ImplicationExpr expr)
    {
        bool result = !expr.left.Visit(this);
        result = expr.right.Visit(this) || result;

        PrintCell(expr, result);
        return result;
    }

    public bool VisitDoubleImplication(DoubleImplicationExpr expr)
    {
        bool resultLeft = !expr.left.Visit(this);
        bool resultRight = !expr.right.Visit(this);

        bool leftImplRight = !resultLeft || resultRight;
        bool rightImplLeft = !resultRight || resultLeft;
        bool result = leftImplRight && rightImplLeft;

        PrintCell(expr, result);
        return result;
    }

    public bool VisitOr(OrExpr expr)
    {
        bool result = expr.left.Visit(this);
        result = expr.right.Visit(this) || result;

        PrintCell(expr, result);
        return result;
    }

    public bool VisitAnd(AndExpr expr)
    {
        bool result = expr.left.Visit(this);
        result = expr.right.Visit(this) && result;

        PrintCell(expr, result);
        return result;
    }

    public bool VisitConstant(ConstantExpr expr)
    {
        PrintCell(expr, expr.value);
        return expr.value;
    }

    public static void PrintTruthTable(IExpr expr, params string[] args)
    {
        new Evaluator(args).Eval(expr);
    }
}
