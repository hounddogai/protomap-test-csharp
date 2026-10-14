namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of training due audit policy.</summary>
public static class TrainingDueAuditPolicy
{
    public static int TrainingDueAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    public static string TrainingDueAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 2578.0, in order.</summary>
    public static int TrainingDueAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 2578.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
