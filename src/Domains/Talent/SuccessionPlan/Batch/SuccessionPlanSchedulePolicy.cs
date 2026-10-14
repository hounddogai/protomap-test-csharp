namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan schedule policy.</summary>
public static class SuccessionPlanSchedulePolicy
{
    public static double SuccessionPlanScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a succession plan schedule code in its canonical SPS- form.</summary>
    public static string NormalizeSuccessionPlanScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
