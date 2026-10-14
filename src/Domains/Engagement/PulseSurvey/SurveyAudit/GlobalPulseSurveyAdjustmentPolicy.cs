namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global pulse survey adjustment policy.</summary>
public static class GlobalPulseSurveyAdjustmentPolicy
{
    /// <summary>Puts a global pulse survey adjustment code in its canonical GPSA- form.</summary>
    public static string NormalizeGlobalPulseSurveyAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GlobalPulseSurveyAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int GlobalPulseSurveyAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
