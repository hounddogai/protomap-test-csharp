namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary policy item policy.</summary>
public static class PrimaryPolicyItemPolicy
{
    /// <summary>Names the band a primary policy item score falls in.</summary>
    public static string ClassifyPrimaryPolicyItem(double score)
    {
        if (score < 29.7)
        {
            return "critical";
        }
        if (score < 46.5)
        {
            return "minimal";
        }
        if (score < 91.7)
        {
            return "watch";
        }
        return "low";
    }

    public static double PrimaryPolicyItemWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PrimaryPolicyItemSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryPolicyItem()
    {
        var policy = Retry.DefaultPolicy("primary-policy-item");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
