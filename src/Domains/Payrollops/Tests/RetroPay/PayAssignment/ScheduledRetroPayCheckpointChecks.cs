namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class ScheduledRetroPayCheckpointChecks
{
    public static void ScheduledRetroPayCheckpointStartsInNew()
    {
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("code-164", "label-438");
        Check.Equal(item.Status, ScheduledRetroPayCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledRetroPayCheckpointMovesToQueued()
    {
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("code-164", "label-438");
        try
        {
            item.MoveTo(ScheduledRetroPayCheckpointStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledRetroPayCheckpointStatus.Queued, "status");
    }

    public static void ScheduledRetroPayCheckpointRejectsMovesFromFailed()
    {
        var item = ScheduledRetroPayCheckpoint.NewScheduledRetroPayCheckpoint("code-164", "label-438");
        item.Status = ScheduledRetroPayCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledRetroPayCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledRetroPayCheckpointStartsInNew();
        ScheduledRetroPayCheckpointMovesToQueued();
        ScheduledRetroPayCheckpointRejectsMovesFromFailed();
    }
}
