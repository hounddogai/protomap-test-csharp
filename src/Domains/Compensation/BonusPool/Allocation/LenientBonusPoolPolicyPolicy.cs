namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient bonus pool policy policy.</summary>
public static class LenientBonusPoolPolicyPolicy
{
    public static double LenientBonusPoolPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a lenient bonus pool policy score falls in.</summary>
    public static string ClassifyLenientBonusPoolPolicy(double score)
    {
        if (score < 36.8)
        {
            return "medium";
        }
        if (score < 38.5)
        {
            return "watch";
        }
        return "low";
    }

    public static string LenientBonusPoolPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientBonusPoolPolicy()
    {
        var policy = Retry.DefaultPolicy("lenient-bonus-pool-policy");
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
