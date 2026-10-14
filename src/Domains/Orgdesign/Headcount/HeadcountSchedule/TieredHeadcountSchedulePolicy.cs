namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered headcount schedule policy.</summary>
public static class TieredHeadcountSchedulePolicy
{
    /// <summary>Puts a tiered headcount schedule code in its canonical THS- form.</summary>
    public static string NormalizeTieredHeadcountScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("THS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"THS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TieredHeadcountScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
