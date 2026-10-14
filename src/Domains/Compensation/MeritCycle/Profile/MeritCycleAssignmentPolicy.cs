namespace Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Functions of merit cycle assignment policy.</summary>
public static class MeritCycleAssignmentPolicy
{
    /// <summary>Puts a merit cycle assignment code in its canonical MCA- form.</summary>
    public static string NormalizeMeritCycleAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int MeritCycleAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 70062.0, in order.</summary>
    public static int MeritCycleAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70062.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
