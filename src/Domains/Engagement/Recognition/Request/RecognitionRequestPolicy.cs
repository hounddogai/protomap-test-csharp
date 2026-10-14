namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition request policy.</summary>
public static class RecognitionRequestPolicy
{
    public static string RecognitionRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a recognition request code in its canonical RR- form.</summary>
    public static string NormalizeRecognitionRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a recognition request score falls in.</summary>
    public static string ClassifyRecognitionRequest(double score)
    {
        if (score < 65.1)
        {
            return "minimal";
        }
        if (score < 83.6)
        {
            return "low";
        }
        return "medium";
    }
}
