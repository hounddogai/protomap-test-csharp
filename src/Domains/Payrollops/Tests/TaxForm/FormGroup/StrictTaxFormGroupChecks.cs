namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class StrictTaxFormGroupChecks
{
    public static void StrictTaxFormGroupStartsInOpen()
    {
        var item = StrictTaxFormGroup.NewStrictTaxFormGroup("code-342", "label-670");
        Check.Equal(item.Status, StrictTaxFormGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictTaxFormGroupMovesToReopened()
    {
        var item = StrictTaxFormGroup.NewStrictTaxFormGroup("code-342", "label-670");
        try
        {
            item.MoveTo(StrictTaxFormGroupStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictTaxFormGroupStatus.Reopened, "status");
    }

    public static void StrictTaxFormGroupRejectsMovesFromArchived()
    {
        var item = StrictTaxFormGroup.NewStrictTaxFormGroup("code-342", "label-670");
        item.Status = StrictTaxFormGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictTaxFormGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictTaxFormGroupStartsInOpen();
        StrictTaxFormGroupMovesToReopened();
        StrictTaxFormGroupRejectsMovesFromArchived();
    }
}
