namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict floor plan draft policy.</summary>
public static class StrictFloorPlanDraftPolicy
{
    public static double StrictFloorPlanDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.23;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a strict floor plan draft code in its canonical SFPD- form.</summary>
    public static string NormalizeStrictFloorPlanDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string StrictFloorPlanDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
