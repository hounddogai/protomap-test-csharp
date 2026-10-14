namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TaxFormExceptionChecks
{
    public static void TaxFormExceptionStartsInOpen()
    {
        var item = TaxFormException.NewTaxFormException("code-232", "title-21");
        Check.Equal(item.Status, TaxFormExceptionStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormExceptionMovesToReopened()
    {
        var item = TaxFormException.NewTaxFormException("code-232", "title-21");
        try
        {
            item.MoveTo(TaxFormExceptionStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormExceptionStatus.Reopened, "status");
    }

    public static void TaxFormExceptionRejectsMovesFromArchived()
    {
        var item = TaxFormException.NewTaxFormException("code-232", "title-21");
        item.Status = TaxFormExceptionStatus.Archived;
        Check.Fails(() => item.MoveTo(TaxFormExceptionStatus.Open), "move");
    }

    public static void RunAll()
    {
        TaxFormExceptionStartsInOpen();
        TaxFormExceptionMovesToReopened();
        TaxFormExceptionRejectsMovesFromArchived();
    }
}
