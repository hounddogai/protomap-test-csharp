namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AuditScheduleChecks
{
    public static void AuditScheduleStartsInProposed()
    {
        var item = AuditSchedule.NewAuditSchedule("code-60", "title-689");
        Check.Equal(item.Status, AuditScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditScheduleMovesToApproved()
    {
        var item = AuditSchedule.NewAuditSchedule("code-60", "title-689");
        try
        {
            item.MoveTo(AuditScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditScheduleStatus.Approved, "status");
    }

    public static void AuditScheduleRejectsMovesFromWithdrawn()
    {
        var item = AuditSchedule.NewAuditSchedule("code-60", "title-689");
        item.Status = AuditScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AuditScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AuditScheduleStartsInProposed();
        AuditScheduleMovesToApproved();
        AuditScheduleRejectsMovesFromWithdrawn();
    }
}
