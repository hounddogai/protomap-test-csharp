namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of floor plan request policy.</summary>
public static class FloorPlanRequestPolicy
{
    public static string FloorPlanRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int FloorPlanRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a floor plan request code in its canonical FPR- form.</summary>
    public static string NormalizeFloorPlanRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
