namespace Acme.Workforce.Absence.Policy.Budget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient policy quota policy.</summary>
public static class LenientPolicyQuotaPolicy
{
    /// <summary>Names the band a lenient policy quota score falls in.</summary>
    public static string ClassifyLenientPolicyQuota(double score)
    {
        if (score < 24.5)
        {
            return "critical";
        }
        if (score < 42.9)
        {
            return "minimal";
        }
        if (score < 54.4)
        {
            return "watch";
        }
        if (score < 68.0)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientPolicyQuota()
    {
        var policy = Retry.DefaultPolicy("lenient-policy-quota");
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

    public static double LenientPolicyQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
