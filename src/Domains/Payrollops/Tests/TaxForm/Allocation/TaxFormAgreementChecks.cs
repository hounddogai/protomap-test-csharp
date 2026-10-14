namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAgreementChecks
{
    public static void TaxFormAgreementStartsInDraft()
    {
        var item = TaxFormAgreement.NewTaxFormAgreement("code-74", "title-745");
        Check.Equal(item.Status, TaxFormAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormAgreementMovesToPending()
    {
        var item = TaxFormAgreement.NewTaxFormAgreement("code-74", "title-745");
        try
        {
            item.MoveTo(TaxFormAgreementStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormAgreementStatus.Pending, "status");
    }

    public static void TaxFormAgreementRejectsMovesFromClosed()
    {
        var item = TaxFormAgreement.NewTaxFormAgreement("code-74", "title-745");
        item.Status = TaxFormAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(TaxFormAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TaxFormAgreementStartsInDraft();
        TaxFormAgreementMovesToPending();
        TaxFormAgreementRejectsMovesFromClosed();
    }
}
