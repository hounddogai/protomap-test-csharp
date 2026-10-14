namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionAgreementChecks
{
    public static void DeductionAgreementStartsInProposed()
    {
        var item = DeductionAgreement.NewDeductionAgreement("code-145", "label-982");
        Check.Equal(item.Status, DeductionAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionAgreementMovesToApproved()
    {
        var item = DeductionAgreement.NewDeductionAgreement("code-145", "label-982");
        try
        {
            item.MoveTo(DeductionAgreementStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionAgreementStatus.Approved, "status");
    }

    public static void DeductionAgreementRejectsMovesFromWithdrawn()
    {
        var item = DeductionAgreement.NewDeductionAgreement("code-145", "label-982");
        item.Status = DeductionAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeductionAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeductionAgreementStartsInProposed();
        DeductionAgreementMovesToApproved();
        DeductionAgreementRejectsMovesFromWithdrawn();
    }
}
