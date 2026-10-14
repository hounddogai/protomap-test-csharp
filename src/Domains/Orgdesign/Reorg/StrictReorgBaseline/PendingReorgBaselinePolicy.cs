namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending reorg baseline policy.</summary>
public static class PendingReorgBaselinePolicy
{
    /// <summary>Puts a pending reorg baseline code in its canonical PRB- form.</summary>
    public static string NormalizePendingReorgBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingReorgBaseline()
    {
        var policy = Retry.DefaultPolicy("pending-reorg-baseline");
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

    /// <summary>Counts how many amounts fit in a budget of 77052.0, in order.</summary>
    public static int PendingReorgBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77052.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
