namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TaxFormBaselineChecks
{
    public static void TaxFormBaselineStartsInOpen()
    {
        var item = TaxFormBaseline.NewTaxFormBaseline("code-209", "title-901");
        Check.Equal(item.Status, TaxFormBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormBaselineMovesToArchived()
    {
        var item = TaxFormBaseline.NewTaxFormBaseline("code-209", "title-901");
        try
        {
            item.MoveTo(TaxFormBaselineStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormBaselineStatus.Archived, "status");
    }

    public static void TaxFormBaselineRejectsMovesFromArchived()
    {
        var item = TaxFormBaseline.NewTaxFormBaseline("code-209", "title-901");
        item.Status = TaxFormBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(TaxFormBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        TaxFormBaselineStartsInOpen();
        TaxFormBaselineMovesToArchived();
        TaxFormBaselineRejectsMovesFromArchived();
    }
}
