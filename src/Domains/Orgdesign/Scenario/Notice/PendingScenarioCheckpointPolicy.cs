namespace Acme.Workforce.Orgdesign.Scenario.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending scenario checkpoint policy.</summary>
public static class PendingScenarioCheckpointPolicy
{
    public static int PendingScenarioCheckpointSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingScenarioCheckpoint()
    {
        var policy = Retry.DefaultPolicy("pending-scenario-checkpoint");
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
