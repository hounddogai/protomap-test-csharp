namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default training due audit policy.</summary>
public static class DefaultTrainingDueAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 17783.0, in order.</summary>
    public static int DefaultTrainingDueAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17783.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double DefaultTrainingDueAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a default training due audit score falls in.</summary>
    public static string ClassifyDefaultTrainingDueAudit(double score)
    {
        if (score < 16.2)
        {
            return "medium";
        }
        if (score < 18.6)
        {
            return "ok";
        }
        if (score < 46.3)
        {
            return "watch";
        }
        if (score < 65.8)
        {
            return "critical";
        }
        return "elevated";
    }
}
