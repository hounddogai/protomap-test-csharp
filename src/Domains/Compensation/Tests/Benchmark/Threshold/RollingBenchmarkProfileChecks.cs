namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RollingBenchmarkProfileChecks
{
    public static void RollingBenchmarkProfileStartsInIdle()
    {
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("code-486", "title-65");
        Check.Equal(item.Status, RollingBenchmarkProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBenchmarkProfileMovesToDraining()
    {
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("code-486", "title-65");
        try
        {
            item.MoveTo(RollingBenchmarkProfileStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBenchmarkProfileStatus.Draining, "status");
    }

    public static void RollingBenchmarkProfileRejectsMovesFromStopped()
    {
        var item = RollingBenchmarkProfile.NewRollingBenchmarkProfile("code-486", "title-65");
        item.Status = RollingBenchmarkProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingBenchmarkProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingBenchmarkProfileStartsInIdle();
        RollingBenchmarkProfileMovesToDraining();
        RollingBenchmarkProfileRejectsMovesFromStopped();
    }
}
