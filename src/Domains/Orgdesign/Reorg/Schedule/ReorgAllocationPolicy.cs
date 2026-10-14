namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Functions of reorg allocation policy.</summary>
public static class ReorgAllocationPolicy
{
    /// <summary>Puts a reorg allocation code in its canonical RA- form.</summary>
    public static string NormalizeReorgAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 4027.0, in order.</summary>
    public static int ReorgAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4027.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int ReorgAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
