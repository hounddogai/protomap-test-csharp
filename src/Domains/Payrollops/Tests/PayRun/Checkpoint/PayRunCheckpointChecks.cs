namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunCheckpointChecks
{
    public static void PayRunCheckpointStartsInIdle()
    {
        var item = PayRunCheckpoint.NewPayRunCheckpoint("code-528", "label-87");
        Check.Equal(item.Status, PayRunCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunCheckpointMovesToWarming()
    {
        var item = PayRunCheckpoint.NewPayRunCheckpoint("code-528", "label-87");
        try
        {
            item.MoveTo(PayRunCheckpointStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunCheckpointStatus.Warming, "status");
    }

    public static void PayRunCheckpointRejectsMovesFromStopped()
    {
        var item = PayRunCheckpoint.NewPayRunCheckpoint("code-528", "label-87");
        item.Status = PayRunCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(PayRunCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PayRunCheckpointStartsInIdle();
        PayRunCheckpointMovesToWarming();
        PayRunCheckpointRejectsMovesFromStopped();
    }
}
