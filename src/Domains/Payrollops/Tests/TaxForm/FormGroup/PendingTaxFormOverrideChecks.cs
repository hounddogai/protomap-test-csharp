namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class PendingTaxFormOverrideChecks
{
    public static void PendingTaxFormOverrideStartsInPlanned()
    {
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("code-423", "title-560");
        Check.Equal(item.Status, PendingTaxFormOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingTaxFormOverrideMovesToScheduled()
    {
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("code-423", "title-560");
        try
        {
            item.MoveTo(PendingTaxFormOverrideStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingTaxFormOverrideStatus.Scheduled, "status");
    }

    public static void PendingTaxFormOverrideRejectsMovesFromCancelled()
    {
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("code-423", "title-560");
        item.Status = PendingTaxFormOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingTaxFormOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingTaxFormOverrideStartsInPlanned();
        PendingTaxFormOverrideMovesToScheduled();
        PendingTaxFormOverrideRejectsMovesFromCancelled();
    }
}
