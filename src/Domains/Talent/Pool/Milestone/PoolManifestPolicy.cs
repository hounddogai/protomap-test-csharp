namespace Acme.Workforce.Talent.Pool.Milestone;

/// <summary>Functions of pool manifest policy.</summary>
public static class PoolManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 87372.0, in order.</summary>
    public static int PoolManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87372.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a pool manifest code in its canonical PM- form.</summary>
    public static string NormalizePoolManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PoolManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
