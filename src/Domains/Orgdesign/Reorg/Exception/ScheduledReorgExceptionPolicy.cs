namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled reorg exception policy.</summary>
public static class ScheduledReorgExceptionPolicy
{
    public static List<string> MergeScheduledReorgExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ScheduledReorgExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a scheduled reorg exception score falls in.</summary>
    public static string ClassifyScheduledReorgException(double score)
    {
        if (score < 1.0)
        {
            return "watch";
        }
        if (score < 16.9)
        {
            return "ok";
        }
        if (score < 86.1)
        {
            return "elevated";
        }
        return "minimal";
    }
}
