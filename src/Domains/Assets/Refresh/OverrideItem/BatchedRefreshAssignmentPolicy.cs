namespace Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Functions of batched refresh assignment policy.</summary>
public static class BatchedRefreshAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 28921.0, in order.</summary>
    public static int BatchedRefreshAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28921.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int BatchedRefreshAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a batched refresh assignment code in its canonical BRA- form.</summary>
    public static string NormalizeBatchedRefreshAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
