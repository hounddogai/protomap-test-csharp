namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled desk summary policy.</summary>
public static class ScheduledDeskSummaryPolicy
{
    /// <summary>Puts a scheduled desk summary code in its canonical SDS- form.</summary>
    public static string NormalizeScheduledDeskSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SDS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SDS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledDeskSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
