namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped visitor window policy.</summary>
public static class CappedVisitorWindowPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedVisitorWindow()
    {
        var policy = Retry.DefaultPolicy("capped-visitor-window");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int CappedVisitorWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 2324.0, in order.</summary>
    public static int CappedVisitorWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 2324.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
