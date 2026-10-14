namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of balance template policy.</summary>
public static class BalanceTemplatePolicy
{
    public static int BalanceTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeBalanceTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string BalanceTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a balance template score falls in.</summary>
    public static string ClassifyBalanceTemplate(double score)
    {
        if (score < 25.4)
        {
            return "low";
        }
        if (score < 69.8)
        {
            return "ok";
        }
        if (score < 74.4)
        {
            return "high";
        }
        return "medium";
    }
}
