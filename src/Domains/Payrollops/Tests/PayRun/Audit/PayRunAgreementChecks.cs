namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PayRunAgreementChecks
{
    public static void PayRunAgreementStartsInCreated()
    {
        var item = PayRunAgreement.NewPayRunAgreement("code-816", "name-763");
        Check.Equal(item.Status, PayRunAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunAgreementMovesToRetired()
    {
        var item = PayRunAgreement.NewPayRunAgreement("code-816", "name-763");
        try
        {
            item.MoveTo(PayRunAgreementStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunAgreementStatus.Retired, "status");
    }

    public static void PayRunAgreementRejectsMovesFromRetired()
    {
        var item = PayRunAgreement.NewPayRunAgreement("code-816", "name-763");
        item.Status = PayRunAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(PayRunAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayRunAgreementStartsInCreated();
        PayRunAgreementMovesToRetired();
        PayRunAgreementRejectsMovesFromRetired();
    }
}
