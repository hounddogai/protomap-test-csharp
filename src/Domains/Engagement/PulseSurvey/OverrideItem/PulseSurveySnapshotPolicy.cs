namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey snapshot policy.</summary>
public static class PulseSurveySnapshotPolicy
{
    /// <summary>Puts a pulse survey snapshot code in its canonical PSS- form.</summary>
    public static string NormalizePulseSurveySnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PulseSurveySnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double PulseSurveySnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pulse survey snapshot score falls in.</summary>
    public static string ClassifyPulseSurveySnapshot(double score)
    {
        if (score < 28.1)
        {
            return "critical";
        }
        if (score < 56.2)
        {
            return "medium";
        }
        if (score < 80.9)
        {
            return "minimal";
        }
        if (score < 88.3)
        {
            return "elevated";
        }
        return "high";
    }
}
