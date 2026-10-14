namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement forecast policy.</summary>
public static class EngagementForecastPolicy
{
    public static int EngagementForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a engagement forecast code in its canonical EF- form.</summary>
    public static string NormalizeEngagementForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double EngagementForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
