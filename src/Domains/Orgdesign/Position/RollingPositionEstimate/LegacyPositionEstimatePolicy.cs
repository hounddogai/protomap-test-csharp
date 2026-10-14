namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy position estimate policy.</summary>
public static class LegacyPositionEstimatePolicy
{
    /// <summary>Puts a legacy position estimate code in its canonical LPE- form.</summary>
    public static string NormalizeLegacyPositionEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyPositionEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double LegacyPositionEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.31;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
