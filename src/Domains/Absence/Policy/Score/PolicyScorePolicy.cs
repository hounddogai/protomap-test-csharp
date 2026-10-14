namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy score policy.</summary>
public static class PolicyScorePolicy
{
    public static List<string> MergePolicyScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 28250.0, in order.</summary>
    public static int PolicyScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28250.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a policy score score falls in.</summary>
    public static string ClassifyPolicyScore(double score)
    {
        if (score < 7.5)
        {
            return "elevated";
        }
        if (score < 86.3)
        {
            return "ok";
        }
        return "watch";
    }
}
