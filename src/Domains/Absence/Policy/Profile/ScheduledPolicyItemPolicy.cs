namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled policy item policy.</summary>
public static class ScheduledPolicyItemPolicy
{
    public static double ScheduledPolicyItemWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 69577.0, in order.</summary>
    public static int ScheduledPolicyItemBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69577.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a scheduled policy item score falls in.</summary>
    public static string ClassifyScheduledPolicyItem(double score)
    {
        if (score < 4.9)
        {
            return "critical";
        }
        if (score < 14.7)
        {
            return "low";
        }
        if (score < 63.3)
        {
            return "high";
        }
        if (score < 98.9)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPolicyItem()
    {
        var policy = Retry.DefaultPolicy("scheduled-policy-item");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
