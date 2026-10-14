namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled merit cycle snapshot policy.</summary>
public static class ScheduledMeritCycleSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 24243.0, in order.</summary>
    public static int ScheduledMeritCycleSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24243.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a scheduled merit cycle snapshot code in its canonical SMCS- form.</summary>
    public static string NormalizeScheduledMeritCycleSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SMCS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SMCS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledMeritCycleSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
