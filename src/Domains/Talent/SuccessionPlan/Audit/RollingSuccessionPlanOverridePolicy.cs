namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling succession plan override policy.</summary>
public static class RollingSuccessionPlanOverridePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 91890.0, in order.</summary>
    public static int RollingSuccessionPlanOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91890.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RollingSuccessionPlanOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a rolling succession plan override score falls in.</summary>
    public static string ClassifyRollingSuccessionPlanOverride(double score)
    {
        if (score < 3.9)
        {
            return "watch";
        }
        if (score < 24.2)
        {
            return "critical";
        }
        if (score < 38.9)
        {
            return "high";
        }
        if (score < 92.5)
        {
            return "ok";
        }
        return "minimal";
    }

    public static List<string> MergeRollingSuccessionPlanOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
