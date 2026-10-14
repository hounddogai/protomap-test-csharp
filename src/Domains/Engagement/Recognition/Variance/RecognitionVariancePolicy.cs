namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of recognition variance policy.</summary>
public static class RecognitionVariancePolicy
{
    public static string RecognitionVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a recognition variance score falls in.</summary>
    public static string ClassifyRecognitionVariance(double score)
    {
        if (score < 23.8)
        {
            return "watch";
        }
        if (score < 85.8)
        {
            return "minimal";
        }
        return "medium";
    }

    public static List<string> MergeRecognitionVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int RecognitionVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
