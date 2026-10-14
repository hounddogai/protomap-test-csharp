namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling bonus pool template policy.</summary>
public static class RollingBonusPoolTemplatePolicy
{
    /// <summary>Names the band a rolling bonus pool template score falls in.</summary>
    public static string ClassifyRollingBonusPoolTemplate(double score)
    {
        if (score < 24.5)
        {
            return "ok";
        }
        if (score < 78.0)
        {
            return "critical";
        }
        return "minimal";
    }

    public static List<string> MergeRollingBonusPoolTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RollingBonusPoolTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 66409.0, in order.</summary>
    public static int RollingBonusPoolTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66409.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
