namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleAuditChecks
{
    public static void MeritCycleAuditStartsInProposed()
    {
        var item = MeritCycleAudit.NewMeritCycleAudit("code-281", "name-494");
        Check.Equal(item.Status, MeritCycleAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleAuditMovesToApproved()
    {
        var item = MeritCycleAudit.NewMeritCycleAudit("code-281", "name-494");
        try
        {
            item.MoveTo(MeritCycleAuditStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleAuditStatus.Approved, "status");
    }

    public static void MeritCycleAuditRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleAudit.NewMeritCycleAudit("code-281", "name-494");
        item.Status = MeritCycleAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleAuditStartsInProposed();
        MeritCycleAuditMovesToApproved();
        MeritCycleAuditRejectsMovesFromWithdrawn();
    }
}
