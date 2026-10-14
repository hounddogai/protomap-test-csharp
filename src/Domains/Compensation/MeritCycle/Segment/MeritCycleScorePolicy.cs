namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of merit cycle score policy.</summary>
public static class MeritCycleScorePolicy
{
    public static double MeritCycleScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a merit cycle score code in its canonical MCS- form.</summary>
    public static string NormalizeMeritCycleScoreCode(string code)
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

    public static int MeritCycleScoreSpan(int start, int end)
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

    public static string MeritCycleScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
