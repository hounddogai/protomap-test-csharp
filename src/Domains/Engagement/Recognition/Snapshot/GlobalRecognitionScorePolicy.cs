namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global recognition score policy.</summary>
public static class GlobalRecognitionScorePolicy
{
    public static string GlobalRecognitionScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a global recognition score code in its canonical GRS- form.</summary>
    public static string NormalizeGlobalRecognitionScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double GlobalRecognitionScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
