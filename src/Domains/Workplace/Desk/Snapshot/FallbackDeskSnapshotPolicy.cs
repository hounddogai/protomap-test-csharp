namespace Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Functions of fallback desk snapshot policy.</summary>
public static class FallbackDeskSnapshotPolicy
{
    /// <summary>Puts a fallback desk snapshot code in its canonical FDS- form.</summary>
    public static string NormalizeFallbackDeskSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FDS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FDS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int FallbackDeskSnapshotSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 94911.0, in order.</summary>
    public static int FallbackDeskSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 94911.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
