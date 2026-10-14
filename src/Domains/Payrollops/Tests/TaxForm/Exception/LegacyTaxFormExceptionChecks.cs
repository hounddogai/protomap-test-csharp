namespace Acme.Workforce.Payrollops.Tests.TaxForm.Exception;

using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LegacyTaxFormExceptionChecks
{
    public static void LegacyTaxFormExceptionStartsInPlanned()
    {
        var item = LegacyTaxFormException.NewLegacyTaxFormException("code-775", "title-720");
        Check.Equal(item.Status, LegacyTaxFormExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTaxFormExceptionMovesToInProgress()
    {
        var item = LegacyTaxFormException.NewLegacyTaxFormException("code-775", "title-720");
        try
        {
            item.MoveTo(LegacyTaxFormExceptionStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTaxFormExceptionStatus.InProgress, "status");
    }

    public static void LegacyTaxFormExceptionRejectsMovesFromCancelled()
    {
        var item = LegacyTaxFormException.NewLegacyTaxFormException("code-775", "title-720");
        item.Status = LegacyTaxFormExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyTaxFormExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyTaxFormExceptionStartsInPlanned();
        LegacyTaxFormExceptionMovesToInProgress();
        LegacyTaxFormExceptionRejectsMovesFromCancelled();
    }
}
