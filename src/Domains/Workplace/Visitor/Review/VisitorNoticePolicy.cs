namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor notice policy.</summary>
public static class VisitorNoticePolicy
{
    /// <summary>Names the band a visitor notice score falls in.</summary>
    public static string ClassifyVisitorNotice(double score)
    {
        if (score < 10.5)
        {
            return "critical";
        }
        if (score < 20.4)
        {
            return "high";
        }
        if (score < 21.6)
        {
            return "elevated";
        }
        if (score < 37.7)
        {
            return "watch";
        }
        return "medium";
    }

    public static List<string> MergeVisitorNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 72112.0, in order.</summary>
    public static int VisitorNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72112.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double VisitorNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
