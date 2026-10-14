namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of balance digest policy.</summary>
public static class BalanceDigestPolicy
{
    public static int BalanceDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a balance digest score falls in.</summary>
    public static string ClassifyBalanceDigest(double score)
    {
        if (score < 10.9)
        {
            return "ok";
        }
        if (score < 12.7)
        {
            return "medium";
        }
        if (score < 49.1)
        {
            return "watch";
        }
        if (score < 77.1)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBalanceDigest()
    {
        var policy = Retry.DefaultPolicy("balance-digest");
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

    public static double BalanceDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
