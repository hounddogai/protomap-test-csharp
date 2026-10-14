namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback pulse survey segment policy.</summary>
public static class FallbackPulseSurveySegmentPolicy
{
    public static int FallbackPulseSurveySegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double FallbackPulseSurveySegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a fallback pulse survey segment code in its canonical FPSS- form.</summary>
    public static string NormalizeFallbackPulseSurveySegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPSS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPSS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
