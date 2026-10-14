namespace Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>Functions of strict occupancy summary policy.</summary>
public static class StrictOccupancySummaryPolicy
{
    /// <summary>Puts a strict occupancy summary code in its canonical SOS- form.</summary>
    public static string NormalizeStrictOccupancySummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int StrictOccupancySummarySpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 65714.0, in order.</summary>
    public static int StrictOccupancySummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65714.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
