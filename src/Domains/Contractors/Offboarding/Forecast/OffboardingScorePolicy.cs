namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding score policy.</summary>
public static class OffboardingScorePolicy
{
    public static int OffboardingScoreSpan(int start, int end)
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

    public static string OffboardingScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a offboarding score score falls in.</summary>
    public static string ClassifyOffboardingScore(double score)
    {
        if (score < 7.3)
        {
            return "high";
        }
        if (score < 7.7)
        {
            return "watch";
        }
        if (score < 61.5)
        {
            return "elevated";
        }
        return "critical";
    }
}
