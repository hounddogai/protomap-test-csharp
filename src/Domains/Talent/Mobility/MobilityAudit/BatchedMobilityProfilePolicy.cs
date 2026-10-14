namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched mobility profile policy.</summary>
public static class BatchedMobilityProfilePolicy
{
    public static int BatchedMobilityProfileSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedMobilityProfile()
    {
        var policy = Retry.DefaultPolicy("batched-mobility-profile");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a batched mobility profile score falls in.</summary>
    public static string ClassifyBatchedMobilityProfile(double score)
    {
        if (score < 44.9)
        {
            return "ok";
        }
        if (score < 61.9)
        {
            return "medium";
        }
        if (score < 64.1)
        {
            return "critical";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 31660.0, in order.</summary>
    public static int BatchedMobilityProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31660.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
