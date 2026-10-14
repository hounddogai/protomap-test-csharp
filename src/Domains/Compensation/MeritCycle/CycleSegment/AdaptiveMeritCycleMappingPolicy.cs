namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive merit cycle mapping policy.</summary>
public static class AdaptiveMeritCycleMappingPolicy
{
    public static double AdaptiveMeritCycleMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a adaptive merit cycle mapping score falls in.</summary>
    public static string ClassifyAdaptiveMeritCycleMapping(double score)
    {
        if (score < 9.0)
        {
            return "minimal";
        }
        if (score < 77.2)
        {
            return "low";
        }
        if (score < 97.9)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Puts a adaptive merit cycle mapping code in its canonical AMCM- form.</summary>
    public static string NormalizeAdaptiveMeritCycleMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AMCM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AMCM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
