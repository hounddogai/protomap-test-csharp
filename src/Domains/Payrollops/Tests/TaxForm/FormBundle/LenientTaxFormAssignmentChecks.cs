namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class LenientTaxFormAssignmentChecks
{
    public static void LenientTaxFormAssignmentStartsInPlanned()
    {
        var item = LenientTaxFormAssignment.NewLenientTaxFormAssignment("code-218", "title-116");
        Check.Equal(item.Status, LenientTaxFormAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientTaxFormAssignmentMovesToScheduled()
    {
        var item = LenientTaxFormAssignment.NewLenientTaxFormAssignment("code-218", "title-116");
        try
        {
            item.MoveTo(LenientTaxFormAssignmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientTaxFormAssignmentStatus.Scheduled, "status");
    }

    public static void LenientTaxFormAssignmentRejectsMovesFromCancelled()
    {
        var item = LenientTaxFormAssignment.NewLenientTaxFormAssignment("code-218", "title-116");
        item.Status = LenientTaxFormAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientTaxFormAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientTaxFormAssignmentStartsInPlanned();
        LenientTaxFormAssignmentMovesToScheduled();
        LenientTaxFormAssignmentRejectsMovesFromCancelled();
    }
}
