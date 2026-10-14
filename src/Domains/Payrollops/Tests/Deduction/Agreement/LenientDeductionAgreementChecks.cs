namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionAgreementChecks
{
    public static void LenientDeductionAgreementStartsInProposed()
    {
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("code-859", "title-29");
        Check.Equal(item.Status, LenientDeductionAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientDeductionAgreementMovesToRejected()
    {
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("code-859", "title-29");
        try
        {
            item.MoveTo(LenientDeductionAgreementStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientDeductionAgreementStatus.Rejected, "status");
    }

    public static void LenientDeductionAgreementRejectsMovesFromWithdrawn()
    {
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("code-859", "title-29");
        item.Status = LenientDeductionAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientDeductionAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientDeductionAgreementStartsInProposed();
        LenientDeductionAgreementMovesToRejected();
        LenientDeductionAgreementRejectsMovesFromWithdrawn();
    }
}
