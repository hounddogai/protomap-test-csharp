namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending desk entry policy.</summary>
public static class PendingDeskEntryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingDeskEntry()
    {
        var policy = Retry.DefaultPolicy("pending-desk-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 6749.0, in order.</summary>
    public static int PendingDeskEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6749.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pending desk entry score falls in.</summary>
    public static string ClassifyPendingDeskEntry(double score)
    {
        if (score < 41.3)
        {
            return "medium";
        }
        if (score < 42.0)
        {
            return "watch";
        }
        if (score < 54.4)
        {
            return "critical";
        }
        if (score < 98.2)
        {
            return "ok";
        }
        return "elevated";
    }

    public static double PendingDeskEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.02;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
