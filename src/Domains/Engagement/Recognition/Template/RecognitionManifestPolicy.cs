namespace Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Functions of recognition manifest policy.</summary>
public static class RecognitionManifestPolicy
{
    public static int RecognitionManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a recognition manifest code in its canonical RM- form.</summary>
    public static string NormalizeRecognitionManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a recognition manifest score falls in.</summary>
    public static string ClassifyRecognitionManifest(double score)
    {
        if (score < 15.7)
        {
            return "watch";
        }
        if (score < 59.5)
        {
            return "ok";
        }
        return "low";
    }
}
