namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition policy policy.</summary>
public static class RecognitionPolicyPolicy
{
    /// <summary>Puts a recognition policy code in its canonical RP- form.</summary>
    public static string NormalizeRecognitionPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a recognition policy score falls in.</summary>
    public static string ClassifyRecognitionPolicy(double score)
    {
        if (score < 14.8)
        {
            return "high";
        }
        if (score < 50.6)
        {
            return "low";
        }
        if (score < 63.2)
        {
            return "watch";
        }
        if (score < 99.1)
        {
            return "medium";
        }
        return "minimal";
    }

    public static double RecognitionPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
