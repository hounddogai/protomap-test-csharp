namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientAuditPlanChecks
{
    public static void LenientAuditPlanStartsInProposed()
    {
        var item = LenientAuditPlan.NewLenientAuditPlan("code-474", "label-825");
        Check.Equal(item.Status, LenientAuditPlanStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientAuditPlanMovesToApproved()
    {
        var item = LenientAuditPlan.NewLenientAuditPlan("code-474", "label-825");
        try
        {
            item.MoveTo(LenientAuditPlanStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientAuditPlanStatus.Approved, "status");
    }

    public static void LenientAuditPlanRejectsMovesFromWithdrawn()
    {
        var item = LenientAuditPlan.NewLenientAuditPlan("code-474", "label-825");
        item.Status = LenientAuditPlanStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientAuditPlanStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientAuditPlanStartsInProposed();
        LenientAuditPlanMovesToApproved();
        LenientAuditPlanRejectsMovesFromWithdrawn();
    }
}
