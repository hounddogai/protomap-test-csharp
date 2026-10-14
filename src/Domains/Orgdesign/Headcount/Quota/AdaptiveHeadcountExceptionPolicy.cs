namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive headcount exception policy.</summary>
public static class AdaptiveHeadcountExceptionPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveHeadcountException()
    {
        var policy = Retry.DefaultPolicy("adaptive-headcount-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 19826.0, in order.</summary>
    public static int AdaptiveHeadcountExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19826.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AdaptiveHeadcountExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
