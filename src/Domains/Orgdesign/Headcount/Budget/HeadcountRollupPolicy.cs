namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of headcount rollup policy.</summary>
public static class HeadcountRollupPolicy
{
    public static int HeadcountRollupSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a headcount rollup score falls in.</summary>
    public static string ClassifyHeadcountRollup(double score)
    {
        if (score < 3.1)
        {
            return "ok";
        }
        if (score < 53.9)
        {
            return "watch";
        }
        if (score < 88.4)
        {
            return "minimal";
        }
        if (score < 94.8)
        {
            return "elevated";
        }
        return "critical";
    }

    public static double HeadcountRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountRollup()
    {
        var policy = Retry.DefaultPolicy("headcount-rollup");
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
