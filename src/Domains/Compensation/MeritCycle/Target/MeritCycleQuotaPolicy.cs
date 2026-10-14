namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle quota policy.</summary>
public static class MeritCycleQuotaPolicy
{
    public static List<string> MergeMeritCycleQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double MeritCycleQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a merit cycle quota score falls in.</summary>
    public static string ClassifyMeritCycleQuota(double score)
    {
        if (score < 32.6)
        {
            return "medium";
        }
        if (score < 59.1)
        {
            return "ok";
        }
        if (score < 68.4)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 11847.0, in order.</summary>
    public static int MeritCycleQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 11847.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
