namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormAssignmentChecks
{
    public static void TaxFormAssignmentStartsInDraft()
    {
        var item = TaxFormAssignment.NewTaxFormAssignment("code-111", "title-385");
        Check.Equal(item.Status, TaxFormAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormAssignmentMovesToPending()
    {
        var item = TaxFormAssignment.NewTaxFormAssignment("code-111", "title-385");
        try
        {
            item.MoveTo(TaxFormAssignmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormAssignmentStatus.Pending, "status");
    }

    public static void TaxFormAssignmentRejectsMovesFromClosed()
    {
        var item = TaxFormAssignment.NewTaxFormAssignment("code-111", "title-385");
        item.Status = TaxFormAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(TaxFormAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TaxFormAssignmentStartsInDraft();
        TaxFormAssignmentMovesToPending();
        TaxFormAssignmentRejectsMovesFromClosed();
    }
}
