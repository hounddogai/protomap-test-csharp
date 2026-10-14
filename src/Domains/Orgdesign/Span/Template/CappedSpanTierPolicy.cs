namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped span tier policy.</summary>
public static class CappedSpanTierPolicy
{
    public static int CappedSpanTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a capped span tier score falls in.</summary>
    public static string ClassifyCappedSpanTier(double score)
    {
        if (score < 22.8)
        {
            return "medium";
        }
        if (score < 23.1)
        {
            return "ok";
        }
        if (score < 86.4)
        {
            return "elevated";
        }
        return "high";
    }

    public static double CappedSpanTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
