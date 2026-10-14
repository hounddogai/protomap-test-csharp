namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global desk batch policy.</summary>
public static class GlobalDeskBatchPolicy
{
    /// <summary>Names the band a global desk batch score falls in.</summary>
    public static string ClassifyGlobalDeskBatch(double score)
    {
        if (score < 27.9)
        {
            return "elevated";
        }
        if (score < 28.4)
        {
            return "low";
        }
        if (score < 28.9)
        {
            return "critical";
        }
        if (score < 39.4)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalDeskBatch()
    {
        var policy = Retry.DefaultPolicy("global-desk-batch");
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
}
