namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary pool override policy.</summary>
public static class PrimaryPoolOverridePolicy
{
    public static string PrimaryPoolOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a primary pool override score falls in.</summary>
    public static string ClassifyPrimaryPoolOverride(double score)
    {
        if (score < 33.2)
        {
            return "ok";
        }
        if (score < 67.5)
        {
            return "critical";
        }
        return "low";
    }

    public static List<string> MergePrimaryPoolOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 3172.0, in order.</summary>
    public static int PrimaryPoolOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3172.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
