namespace Acme.Workforce.Compensation.Tests.Adjustment.Forecast;

using Acme.Workforce.Compensation.Adjustment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class FallbackAdjustmentCheckpointChecks
{
    public static void FallbackAdjustmentCheckpointStartsInIdle()
    {
        var item = FallbackAdjustmentCheckpoint.NewFallbackAdjustmentCheckpoint("code-117", "name-913");
        Check.Equal(item.Status, FallbackAdjustmentCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAdjustmentCheckpointMovesToWarming()
    {
        var item = FallbackAdjustmentCheckpoint.NewFallbackAdjustmentCheckpoint("code-117", "name-913");
        try
        {
            item.MoveTo(FallbackAdjustmentCheckpointStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAdjustmentCheckpointStatus.Warming, "status");
    }

    public static void FallbackAdjustmentCheckpointRejectsMovesFromStopped()
    {
        var item = FallbackAdjustmentCheckpoint.NewFallbackAdjustmentCheckpoint("code-117", "name-913");
        item.Status = FallbackAdjustmentCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackAdjustmentCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackAdjustmentCheckpointStartsInIdle();
        FallbackAdjustmentCheckpointMovesToWarming();
        FallbackAdjustmentCheckpointRejectsMovesFromStopped();
    }
}
