namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition quota policy.</summary>
public static class RecognitionQuotaPolicy
{
    public static string RecognitionQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a recognition quota code in its canonical RQ- form.</summary>
    public static string NormalizeRecognitionQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a recognition quota score falls in.</summary>
    public static string ClassifyRecognitionQuota(double score)
    {
        if (score < 65.4)
        {
            return "high";
        }
        if (score < 82.6)
        {
            return "critical";
        }
        return "medium";
    }
}
