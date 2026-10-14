namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary span score policy.</summary>
public static class PrimarySpanScorePolicy
{
    public static double PrimarySpanScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a primary span score code in its canonical PSS- form.</summary>
    public static string NormalizePrimarySpanScoreCode(string code)
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

    public static string PrimarySpanScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
