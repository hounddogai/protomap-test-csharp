namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive training due claim policy.</summary>
public static class AdaptiveTrainingDueClaimPolicy
{
    /// <summary>Names the band a adaptive training due claim score falls in.</summary>
    public static string ClassifyAdaptiveTrainingDueClaim(double score)
    {
        if (score < 48.8)
        {
            return "ok";
        }
        if (score < 82.1)
        {
            return "high";
        }
        if (score < 99.9)
        {
            return "low";
        }
        return "elevated";
    }

    public static double AdaptiveTrainingDueClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 89615.0, in order.</summary>
    public static int AdaptiveTrainingDueClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89615.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAdaptiveTrainingDueClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
