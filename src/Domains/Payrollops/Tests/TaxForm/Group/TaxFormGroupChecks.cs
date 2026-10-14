namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class TaxFormGroupChecks
{
    public static void TaxFormGroupStartsInRequested()
    {
        var item = TaxFormGroup.NewTaxFormGroup("code-261", "label-563");
        Check.Equal(item.Status, TaxFormGroupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormGroupMovesToGranted()
    {
        var item = TaxFormGroup.NewTaxFormGroup("code-261", "label-563");
        try
        {
            item.MoveTo(TaxFormGroupStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormGroupStatus.Granted, "status");
    }

    public static void TaxFormGroupRejectsMovesFromExpired()
    {
        var item = TaxFormGroup.NewTaxFormGroup("code-261", "label-563");
        item.Status = TaxFormGroupStatus.Expired;
        Check.Fails(() => item.MoveTo(TaxFormGroupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TaxFormGroupStartsInRequested();
        TaxFormGroupMovesToGranted();
        TaxFormGroupRejectsMovesFromExpired();
    }
}
