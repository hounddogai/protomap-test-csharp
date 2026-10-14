namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class CappedTaxFormAssignmentChecks
{
    public static void CappedTaxFormAssignmentStartsInIdle()
    {
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("code-253", "title-778");
        Check.Equal(item.Status, CappedTaxFormAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedTaxFormAssignmentMovesToWarming()
    {
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("code-253", "title-778");
        try
        {
            item.MoveTo(CappedTaxFormAssignmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedTaxFormAssignmentStatus.Warming, "status");
    }

    public static void CappedTaxFormAssignmentRejectsMovesFromStopped()
    {
        var item = CappedTaxFormAssignment.NewCappedTaxFormAssignment("code-253", "title-778");
        item.Status = CappedTaxFormAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedTaxFormAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedTaxFormAssignmentStartsInIdle();
        CappedTaxFormAssignmentMovesToWarming();
        CappedTaxFormAssignmentRejectsMovesFromStopped();
    }
}
