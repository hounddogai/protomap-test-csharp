namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional position policy policy.</summary>
public static class RegionalPositionPolicyPolicy
{
    public static List<string> MergeRegionalPositionPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 41392.0, in order.</summary>
    public static int RegionalPositionPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41392.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a regional position policy score falls in.</summary>
    public static string ClassifyRegionalPositionPolicy(double score)
    {
        if (score < 28.5)
        {
            return "ok";
        }
        if (score < 59.8)
        {
            return "critical";
        }
        return "medium";
    }

    public static string RegionalPositionPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
