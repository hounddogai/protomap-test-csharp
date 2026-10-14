namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive engagement entry policy.</summary>
public static class AdaptiveEngagementEntryPolicy
{
    /// <summary>Puts a adaptive engagement entry code in its canonical AEE- form.</summary>
    public static string NormalizeAdaptiveEngagementEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AEE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AEE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 35820.0, in order.</summary>
    public static int AdaptiveEngagementEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35820.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveEngagementEntry()
    {
        var policy = Retry.DefaultPolicy("adaptive-engagement-entry");
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
}
