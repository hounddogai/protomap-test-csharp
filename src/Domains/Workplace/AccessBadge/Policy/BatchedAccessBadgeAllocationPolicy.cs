namespace Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Functions of batched access badge allocation policy.</summary>
public static class BatchedAccessBadgeAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 19280.0, in order.</summary>
    public static int BatchedAccessBadgeAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19280.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched access badge allocation code in its canonical BABA- form.</summary>
    public static string NormalizeBatchedAccessBadgeAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BABA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BABA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
