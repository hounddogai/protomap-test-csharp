namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition draft policy.</summary>
public static class RecognitionDraftPolicy
{
    /// <summary>Puts a recognition draft code in its canonical RD- form.</summary>
    public static string NormalizeRecognitionDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RecognitionDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RecognitionDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
