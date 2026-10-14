namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive offboarding segment policy.</summary>
public static class AdaptiveOffboardingSegmentPolicy
{
    /// <summary>Puts a adaptive offboarding segment code in its canonical AOS- form.</summary>
    public static string NormalizeAdaptiveOffboardingSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AOS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AOS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a adaptive offboarding segment score falls in.</summary>
    public static string ClassifyAdaptiveOffboardingSegment(double score)
    {
        if (score < 23.3)
        {
            return "medium";
        }
        if (score < 47.5)
        {
            return "high";
        }
        if (score < 78.2)
        {
            return "watch";
        }
        return "ok";
    }

    public static double AdaptiveOffboardingSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
