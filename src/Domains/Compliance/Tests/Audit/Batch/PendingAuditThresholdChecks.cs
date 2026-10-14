namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingAuditThresholdChecks
{
    public static void PendingAuditThresholdStartsInIdle()
    {
        var item = PendingAuditThreshold.NewPendingAuditThreshold("code-417", "title-362");
        Check.Equal(item.Status, PendingAuditThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAuditThresholdMovesToDraining()
    {
        var item = PendingAuditThreshold.NewPendingAuditThreshold("code-417", "title-362");
        try
        {
            item.MoveTo(PendingAuditThresholdStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAuditThresholdStatus.Draining, "status");
    }

    public static void PendingAuditThresholdRejectsMovesFromStopped()
    {
        var item = PendingAuditThreshold.NewPendingAuditThreshold("code-417", "title-362");
        item.Status = PendingAuditThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingAuditThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingAuditThresholdStartsInIdle();
        PendingAuditThresholdMovesToDraining();
        PendingAuditThresholdRejectsMovesFromStopped();
    }
}
