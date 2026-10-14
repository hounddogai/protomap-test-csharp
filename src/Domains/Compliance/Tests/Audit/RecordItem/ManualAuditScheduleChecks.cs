namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ManualAuditScheduleChecks
{
    public static void ManualAuditScheduleStartsInIdle()
    {
        var item = ManualAuditSchedule.NewManualAuditSchedule("code-419", "title-803");
        Check.Equal(item.Status, ManualAuditScheduleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAuditScheduleMovesToStopped()
    {
        var item = ManualAuditSchedule.NewManualAuditSchedule("code-419", "title-803");
        try
        {
            item.MoveTo(ManualAuditScheduleStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAuditScheduleStatus.Stopped, "status");
    }

    public static void ManualAuditScheduleRejectsMovesFromStopped()
    {
        var item = ManualAuditSchedule.NewManualAuditSchedule("code-419", "title-803");
        item.Status = ManualAuditScheduleStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualAuditScheduleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualAuditScheduleStartsInIdle();
        ManualAuditScheduleMovesToStopped();
        ManualAuditScheduleRejectsMovesFromStopped();
    }
}
