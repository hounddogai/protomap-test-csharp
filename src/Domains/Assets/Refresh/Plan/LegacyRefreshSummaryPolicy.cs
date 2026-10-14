namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy refresh summary policy.</summary>
public static class LegacyRefreshSummaryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyRefreshSummary()
    {
        var policy = Retry.DefaultPolicy("legacy-refresh-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int LegacyRefreshSummarySpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a legacy refresh summary score falls in.</summary>
    public static string ClassifyLegacyRefreshSummary(double score)
    {
        if (score < 33.0)
        {
            return "low";
        }
        if (score < 69.5)
        {
            return "elevated";
        }
        return "minimal";
    }
}
