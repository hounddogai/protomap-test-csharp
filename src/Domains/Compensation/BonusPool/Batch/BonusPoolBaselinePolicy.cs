namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool baseline policy.</summary>
public static class BonusPoolBaselinePolicy
{
    public static string BonusPoolBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a bonus pool baseline score falls in.</summary>
    public static string ClassifyBonusPoolBaseline(double score)
    {
        if (score < 5.0)
        {
            return "low";
        }
        if (score < 63.3)
        {
            return "medium";
        }
        if (score < 96.9)
        {
            return "high";
        }
        return "minimal";
    }

    public static List<string> MergeBonusPoolBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 91483.0, in order.</summary>
    public static int BonusPoolBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91483.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
