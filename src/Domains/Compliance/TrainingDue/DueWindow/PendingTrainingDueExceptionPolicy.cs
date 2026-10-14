namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending training due exception policy.</summary>
public static class PendingTrainingDueExceptionPolicy
{
    public static double PendingTrainingDueExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 78469.0, in order.</summary>
    public static int PendingTrainingDueExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78469.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pending training due exception score falls in.</summary>
    public static string ClassifyPendingTrainingDueException(double score)
    {
        if (score < 4.1)
        {
            return "minimal";
        }
        if (score < 13.1)
        {
            return "critical";
        }
        if (score < 18.7)
        {
            return "watch";
        }
        if (score < 61.5)
        {
            return "ok";
        }
        return "low";
    }

    public static List<string> MergePendingTrainingDueExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
