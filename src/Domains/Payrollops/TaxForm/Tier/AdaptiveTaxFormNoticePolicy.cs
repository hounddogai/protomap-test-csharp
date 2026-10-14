namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive tax form notice policy.</summary>
public static class AdaptiveTaxFormNoticePolicy
{
    public static string AdaptiveTaxFormNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int AdaptiveTaxFormNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static List<string> MergeAdaptiveTaxFormNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a adaptive tax form notice score falls in.</summary>
    public static string ClassifyAdaptiveTaxFormNotice(double score)
    {
        if (score < 62.1)
        {
            return "medium";
        }
        if (score < 74.6)
        {
            return "watch";
        }
        if (score < 84.2)
        {
            return "high";
        }
        return "low";
    }
}
