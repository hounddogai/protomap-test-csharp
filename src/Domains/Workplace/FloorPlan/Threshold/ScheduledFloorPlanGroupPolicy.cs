namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Functions of scheduled floor plan group policy.</summary>
public static class ScheduledFloorPlanGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 41100.0, in order.</summary>
    public static int ScheduledFloorPlanGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41100.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int ScheduledFloorPlanGroupSpan(int start, int end)
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

    /// <summary>Puts a scheduled floor plan group code in its canonical SFPG- form.</summary>
    public static string NormalizeScheduledFloorPlanGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFPG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFPG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
