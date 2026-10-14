namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient recognition batch policy.</summary>
public static class LenientRecognitionBatchPolicy
{
    public static List<string> MergeLenientRecognitionBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a lenient recognition batch score falls in.</summary>
    public static string ClassifyLenientRecognitionBatch(double score)
    {
        if (score < 28.4)
        {
            return "medium";
        }
        if (score < 72.3)
        {
            return "ok";
        }
        if (score < 99.1)
        {
            return "high";
        }
        return "low";
    }

    public static double LenientRecognitionBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
