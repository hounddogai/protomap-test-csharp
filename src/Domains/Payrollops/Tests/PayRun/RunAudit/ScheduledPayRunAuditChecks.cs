namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAudit;

using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Checks of run audit code, run by the project's runner.</summary>
public static class ScheduledPayRunAuditChecks
{
    public static void ScheduledPayRunAuditStartsInIdle()
    {
        var item = ScheduledPayRunAudit.NewScheduledPayRunAudit("code-140", "label-979");
        Check.Equal(item.Status, ScheduledPayRunAuditStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayRunAuditMovesToReady()
    {
        var item = ScheduledPayRunAudit.NewScheduledPayRunAudit("code-140", "label-979");
        try
        {
            item.MoveTo(ScheduledPayRunAuditStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayRunAuditStatus.Ready, "status");
    }

    public static void ScheduledPayRunAuditRejectsMovesFromStopped()
    {
        var item = ScheduledPayRunAudit.NewScheduledPayRunAudit("code-140", "label-979");
        item.Status = ScheduledPayRunAuditStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledPayRunAuditStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledPayRunAuditStartsInIdle();
        ScheduledPayRunAuditMovesToReady();
        ScheduledPayRunAuditRejectsMovesFromStopped();
    }
}
