namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class PendingPayRunAgreementChecks
{
    public static void PendingPayRunAgreementStartsInDraft()
    {
        var item = PendingPayRunAgreement.NewPendingPayRunAgreement("code-509", "title-140");
        Check.Equal(item.Status, PendingPayRunAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPayRunAgreementMovesToActive()
    {
        var item = PendingPayRunAgreement.NewPendingPayRunAgreement("code-509", "title-140");
        try
        {
            item.MoveTo(PendingPayRunAgreementStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPayRunAgreementStatus.Active, "status");
    }

    public static void PendingPayRunAgreementRejectsMovesFromClosed()
    {
        var item = PendingPayRunAgreement.NewPendingPayRunAgreement("code-509", "title-140");
        item.Status = PendingPayRunAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingPayRunAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingPayRunAgreementStartsInDraft();
        PendingPayRunAgreementMovesToActive();
        PendingPayRunAgreementRejectsMovesFromClosed();
    }
}
