namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of reorg summary policy.</summary>
public static class ReorgSummaryPolicy
{
    /// <summary>Names the band a reorg summary score falls in.</summary>
    public static string ClassifyReorgSummary(double score)
    {
        if (score < 34.8)
        {
            return "medium";
        }
        if (score < 48.8)
        {
            return "critical";
        }
        if (score < 54.3)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeReorgSummary()
    {
        var policy = Retry.DefaultPolicy("reorg-summary");
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
