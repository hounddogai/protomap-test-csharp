namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient visitor entry policy.</summary>
public static class LenientVisitorEntryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientVisitorEntry()
    {
        var policy = Retry.DefaultPolicy("lenient-visitor-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 63868.0, in order.</summary>
    public static int LenientVisitorEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63868.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient visitor entry score falls in.</summary>
    public static string ClassifyLenientVisitorEntry(double score)
    {
        if (score < 26.2)
        {
            return "ok";
        }
        if (score < 53.4)
        {
            return "elevated";
        }
        if (score < 79.4)
        {
            return "high";
        }
        if (score < 89.1)
        {
            return "low";
        }
        return "watch";
    }

    public static int LenientVisitorEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
