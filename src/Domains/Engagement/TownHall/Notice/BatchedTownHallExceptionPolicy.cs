namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched town hall exception policy.</summary>
public static class BatchedTownHallExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 56060.0, in order.</summary>
    public static int BatchedTownHallExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56060.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedTownHallException()
    {
        var policy = Retry.DefaultPolicy("batched-town-hall-exception");
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

    /// <summary>Puts a batched town hall exception code in its canonical BTHE- form.</summary>
    public static string NormalizeBatchedTownHallExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BTHE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BTHE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
