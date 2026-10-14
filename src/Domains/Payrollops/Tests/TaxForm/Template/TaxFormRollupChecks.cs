namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormRollupChecks
{
    public static void TaxFormRollupStartsInCreated()
    {
        var item = TaxFormRollup.NewTaxFormRollup("code-891", "title-149");
        Check.Equal(item.Status, TaxFormRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormRollupMovesToRetired()
    {
        var item = TaxFormRollup.NewTaxFormRollup("code-891", "title-149");
        try
        {
            item.MoveTo(TaxFormRollupStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormRollupStatus.Retired, "status");
    }

    public static void TaxFormRollupRejectsMovesFromRetired()
    {
        var item = TaxFormRollup.NewTaxFormRollup("code-891", "title-149");
        item.Status = TaxFormRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(TaxFormRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        TaxFormRollupStartsInCreated();
        TaxFormRollupMovesToRetired();
        TaxFormRollupRejectsMovesFromRetired();
    }
}
