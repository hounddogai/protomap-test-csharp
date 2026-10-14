namespace Acme.Workforce.Workplace.Visitor.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient visitor exception policy.</summary>
public static class LenientVisitorExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 85666.0, in order.</summary>
    public static int LenientVisitorExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85666.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a lenient visitor exception code in its canonical LVE- form.</summary>
    public static string NormalizeLenientVisitorExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LVE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LVE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientVisitorException()
    {
        var policy = Retry.DefaultPolicy("lenient-visitor-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
