namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending return plan window policy.</summary>
public static class PendingReturnPlanWindowPolicy
{
    public static double PendingReturnPlanWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 60482.0, in order.</summary>
    public static int PendingReturnPlanWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60482.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pending return plan window score falls in.</summary>
    public static string ClassifyPendingReturnPlanWindow(double score)
    {
        if (score < 82.5)
        {
            return "elevated";
        }
        if (score < 89.1)
        {
            return "ok";
        }
        if (score < 94.2)
        {
            return "low";
        }
        if (score < 99.2)
        {
            return "critical";
        }
        return "minimal";
    }

    public static int PendingReturnPlanWindowSpan(int start, int end)
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
}
