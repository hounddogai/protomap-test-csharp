namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool notice policy.</summary>
public static class BonusPoolNoticePolicy
{
    public static int BonusPoolNoticeSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 67481.0, in order.</summary>
    public static int BonusPoolNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67481.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeBonusPoolNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a bonus pool notice score falls in.</summary>
    public static string ClassifyBonusPoolNotice(double score)
    {
        if (score < 3.9)
        {
            return "critical";
        }
        if (score < 89.3)
        {
            return "minimal";
        }
        return "ok";
    }
}
