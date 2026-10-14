namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment notice policy.</summary>
public static class AdjustmentNoticePolicy
{
    public static int AdjustmentNoticeSpan(int start, int end)
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

    /// <summary>Names the band a adjustment notice score falls in.</summary>
    public static string ClassifyAdjustmentNotice(double score)
    {
        if (score < 18.9)
        {
            return "critical";
        }
        if (score < 39.0)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 24075.0, in order.</summary>
    public static int AdjustmentNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24075.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double AdjustmentNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
