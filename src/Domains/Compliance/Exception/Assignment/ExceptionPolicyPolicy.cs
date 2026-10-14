namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of exception policy policy.</summary>
public static class ExceptionPolicyPolicy
{
    public static string ExceptionPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 52970.0, in order.</summary>
    public static int ExceptionPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52970.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a exception policy score falls in.</summary>
    public static string ClassifyExceptionPolicy(double score)
    {
        if (score < 32.5)
        {
            return "elevated";
        }
        if (score < 96.9)
        {
            return "critical";
        }
        return "low";
    }

    public static List<string> MergeExceptionPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
