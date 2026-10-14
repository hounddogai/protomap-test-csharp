namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Functions of manual recognition schedule policy.</summary>
public static class ManualRecognitionSchedulePolicy
{
    /// <summary>Puts a manual recognition schedule code in its canonical MRS- form.</summary>
    public static string NormalizeManualRecognitionScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a manual recognition schedule score falls in.</summary>
    public static string ClassifyManualRecognitionSchedule(double score)
    {
        if (score < 13.8)
        {
            return "elevated";
        }
        if (score < 72.9)
        {
            return "high";
        }
        return "watch";
    }
}
