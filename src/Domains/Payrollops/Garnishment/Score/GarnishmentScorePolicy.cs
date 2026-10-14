namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of garnishment score policy.</summary>
public static class GarnishmentScorePolicy
{
    public static double GarnishmentScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a garnishment score score falls in.</summary>
    public static string ClassifyGarnishmentScore(double score)
    {
        if (score < 7.3)
        {
            return "ok";
        }
        if (score < 33.6)
        {
            return "medium";
        }
        return "minimal";
    }

    public static int GarnishmentScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGarnishmentScore()
    {
        var policy = Retry.DefaultPolicy("garnishment-score");
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
