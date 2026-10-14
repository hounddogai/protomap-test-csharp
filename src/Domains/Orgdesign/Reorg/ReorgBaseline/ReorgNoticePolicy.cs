namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of reorg notice policy.</summary>
public static class ReorgNoticePolicy
{
    public static int ReorgNoticeSpan(int start, int end)
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
        return span / 60;
    }

    public static List<string> MergeReorgNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a reorg notice score falls in.</summary>
    public static string ClassifyReorgNotice(double score)
    {
        if (score < 38.6)
        {
            return "high";
        }
        if (score < 53.7)
        {
            return "elevated";
        }
        if (score < 78.4)
        {
            return "watch";
        }
        return "minimal";
    }
}
