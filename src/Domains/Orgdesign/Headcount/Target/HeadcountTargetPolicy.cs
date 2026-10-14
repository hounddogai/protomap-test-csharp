namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of headcount target policy.</summary>
public static class HeadcountTargetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74452.0, in order.</summary>
    public static int HeadcountTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74452.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int HeadcountTargetSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergeHeadcountTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a headcount target score falls in.</summary>
    public static string ClassifyHeadcountTarget(double score)
    {
        if (score < 4.4)
        {
            return "low";
        }
        if (score < 20.9)
        {
            return "minimal";
        }
        if (score < 88.2)
        {
            return "watch";
        }
        return "ok";
    }
}
