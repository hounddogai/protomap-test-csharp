namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional recognition snapshot policy.</summary>
public static class RegionalRecognitionSnapshotPolicy
{
    public static int RegionalRecognitionSnapshotSpan(int start, int end)
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

    /// <summary>Names the band a regional recognition snapshot score falls in.</summary>
    public static string ClassifyRegionalRecognitionSnapshot(double score)
    {
        if (score < 25.1)
        {
            return "low";
        }
        if (score < 93.6)
        {
            return "minimal";
        }
        return "watch";
    }

    public static string RegionalRecognitionSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergeRegionalRecognitionSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
