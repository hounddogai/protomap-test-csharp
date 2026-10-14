namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of occupancy rule policy.</summary>
public static class OccupancyRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 43009.0, in order.</summary>
    public static int OccupancyRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43009.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyRule()
    {
        var policy = Retry.DefaultPolicy("occupancy-rule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a occupancy rule code in its canonical OR- form.</summary>
    public static string NormalizeOccupancyRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int OccupancyRuleSpan(int start, int end)
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
        return span / 86400;
    }
}
