namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary visitor override policy.</summary>
public static class PrimaryVisitorOverridePolicy
{
    public static double PrimaryVisitorOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.07;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PrimaryVisitorOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a primary visitor override code in its canonical PVO- form.</summary>
    public static string NormalizePrimaryVisitorOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PVO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PVO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
