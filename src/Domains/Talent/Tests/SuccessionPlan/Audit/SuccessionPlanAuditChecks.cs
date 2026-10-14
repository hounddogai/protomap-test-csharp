namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAuditChecks
{
    public static void SuccessionPlanAuditStartsInDraft()
    {
        var item = SuccessionPlanAudit.NewSuccessionPlanAudit("code-166", "name-870");
        Check.Equal(item.Status, SuccessionPlanAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanAuditMovesToPending()
    {
        var item = SuccessionPlanAudit.NewSuccessionPlanAudit("code-166", "name-870");
        try
        {
            item.MoveTo(SuccessionPlanAuditStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanAuditStatus.Pending, "status");
    }

    public static void SuccessionPlanAuditRejectsMovesFromClosed()
    {
        var item = SuccessionPlanAudit.NewSuccessionPlanAudit("code-166", "name-870");
        item.Status = SuccessionPlanAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(SuccessionPlanAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanAuditStartsInDraft();
        SuccessionPlanAuditMovesToPending();
        SuccessionPlanAuditRejectsMovesFromClosed();
    }
}
