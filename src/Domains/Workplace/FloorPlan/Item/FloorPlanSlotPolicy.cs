namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of floor plan slot policy.</summary>
public static class FloorPlanSlotPolicy
{
    public static double FloorPlanSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a floor plan slot code in its canonical FPS- form.</summary>
    public static string NormalizeFloorPlanSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 57810.0, in order.</summary>
    public static int FloorPlanSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57810.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a floor plan slot score falls in.</summary>
    public static string ClassifyFloorPlanSlot(double score)
    {
        if (score < 14.9)
        {
            return "critical";
        }
        if (score < 80.2)
        {
            return "minimal";
        }
        return "medium";
    }
}
