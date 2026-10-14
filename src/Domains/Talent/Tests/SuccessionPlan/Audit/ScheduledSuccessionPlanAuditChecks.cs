namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ScheduledSuccessionPlanAuditChecks
{
    public static void ScheduledSuccessionPlanAuditStartsInProposed()
    {
        var item = ScheduledSuccessionPlanAudit.NewScheduledSuccessionPlanAudit("code-589", "title-971");
        Check.Equal(item.Status, ScheduledSuccessionPlanAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledSuccessionPlanAuditMovesToRejected()
    {
        var item = ScheduledSuccessionPlanAudit.NewScheduledSuccessionPlanAudit("code-589", "title-971");
        try
        {
            item.MoveTo(ScheduledSuccessionPlanAuditStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledSuccessionPlanAuditStatus.Rejected, "status");
    }

    public static void ScheduledSuccessionPlanAuditRejectsMovesFromWithdrawn()
    {
        var item = ScheduledSuccessionPlanAudit.NewScheduledSuccessionPlanAudit("code-589", "title-971");
        item.Status = ScheduledSuccessionPlanAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledSuccessionPlanAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledSuccessionPlanAuditStartsInProposed();
        ScheduledSuccessionPlanAuditMovesToRejected();
        ScheduledSuccessionPlanAuditRejectsMovesFromWithdrawn();
    }
}
