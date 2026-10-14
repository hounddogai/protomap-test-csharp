namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy floor plan notice policy.</summary>
public static class LegacyFloorPlanNoticePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 57884.0, in order.</summary>
    public static int LegacyFloorPlanNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57884.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeLegacyFloorPlanNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a legacy floor plan notice score falls in.</summary>
    public static string ClassifyLegacyFloorPlanNotice(double score)
    {
        if (score < 57.6)
        {
            return "ok";
        }
        if (score < 71.3)
        {
            return "high";
        }
        if (score < 96.2)
        {
            return "watch";
        }
        return "low";
    }

    public static double LegacyFloorPlanNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
