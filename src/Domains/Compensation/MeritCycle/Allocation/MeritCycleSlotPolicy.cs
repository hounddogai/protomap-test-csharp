namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Functions of merit cycle slot policy.</summary>
public static class MeritCycleSlotPolicy
{
    /// <summary>Puts a merit cycle slot code in its canonical MCS- form.</summary>
    public static string NormalizeMeritCycleSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 26018.0, in order.</summary>
    public static int MeritCycleSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26018.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
