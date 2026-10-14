namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback recognition slot policy.</summary>
public static class FallbackRecognitionSlotPolicy
{
    public static List<string> MergeFallbackRecognitionSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a fallback recognition slot code in its canonical FRS- form.</summary>
    public static string NormalizeFallbackRecognitionSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 13791.0, in order.</summary>
    public static int FallbackRecognitionSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13791.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a fallback recognition slot score falls in.</summary>
    public static string ClassifyFallbackRecognitionSlot(double score)
    {
        if (score < 63.2)
        {
            return "critical";
        }
        if (score < 65.7)
        {
            return "ok";
        }
        if (score < 74.5)
        {
            return "medium";
        }
        if (score < 85.4)
        {
            return "low";
        }
        return "watch";
    }
}
