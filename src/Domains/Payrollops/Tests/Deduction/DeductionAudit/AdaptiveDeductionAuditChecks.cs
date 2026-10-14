namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class AdaptiveDeductionAuditChecks
{
    public static void AdaptiveDeductionAuditStartsInDraft()
    {
        var item = AdaptiveDeductionAudit.NewAdaptiveDeductionAudit("code-334", "label-807");
        Check.Equal(item.Status, AdaptiveDeductionAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveDeductionAuditMovesToSuspended()
    {
        var item = AdaptiveDeductionAudit.NewAdaptiveDeductionAudit("code-334", "label-807");
        try
        {
            item.MoveTo(AdaptiveDeductionAuditStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveDeductionAuditStatus.Suspended, "status");
    }

    public static void AdaptiveDeductionAuditRejectsMovesFromClosed()
    {
        var item = AdaptiveDeductionAudit.NewAdaptiveDeductionAudit("code-334", "label-807");
        item.Status = AdaptiveDeductionAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveDeductionAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveDeductionAuditStartsInDraft();
        AdaptiveDeductionAuditMovesToSuspended();
        AdaptiveDeductionAuditRejectsMovesFromClosed();
    }
}
