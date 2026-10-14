namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive visitor override policy.</summary>
public static class AdaptiveVisitorOverridePolicy
{
    public static double AdaptiveVisitorOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AdaptiveVisitorOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a adaptive visitor override code in its canonical AVO- form.</summary>
    public static string NormalizeAdaptiveVisitorOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AVO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AVO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
