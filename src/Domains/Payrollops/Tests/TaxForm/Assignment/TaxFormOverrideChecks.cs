namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormOverrideChecks
{
    public static void TaxFormOverrideStartsInPlanned()
    {
        var item = TaxFormOverride.NewTaxFormOverride("code-86", "title-537");
        Check.Equal(item.Status, TaxFormOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormOverrideMovesToCancelled()
    {
        var item = TaxFormOverride.NewTaxFormOverride("code-86", "title-537");
        try
        {
            item.MoveTo(TaxFormOverrideStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormOverrideStatus.Cancelled, "status");
    }

    public static void TaxFormOverrideRejectsMovesFromCancelled()
    {
        var item = TaxFormOverride.NewTaxFormOverride("code-86", "title-537");
        item.Status = TaxFormOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TaxFormOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TaxFormOverrideStartsInPlanned();
        TaxFormOverrideMovesToCancelled();
        TaxFormOverrideRejectsMovesFromCancelled();
    }
}
