namespace Acme.Workforce.Payrollops.Tests.TaxForm.Metric;

using Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TaxFormMilestoneChecks
{
    public static void TaxFormMilestoneStartsInPlanned()
    {
        var item = TaxFormMilestone.NewTaxFormMilestone("code-249", "title-929");
        Check.Equal(item.Status, TaxFormMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormMilestoneMovesToCompleted()
    {
        var item = TaxFormMilestone.NewTaxFormMilestone("code-249", "title-929");
        try
        {
            item.MoveTo(TaxFormMilestoneStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormMilestoneStatus.Completed, "status");
    }

    public static void TaxFormMilestoneRejectsMovesFromCancelled()
    {
        var item = TaxFormMilestone.NewTaxFormMilestone("code-249", "title-929");
        item.Status = TaxFormMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TaxFormMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TaxFormMilestoneStartsInPlanned();
        TaxFormMilestoneMovesToCompleted();
        TaxFormMilestoneRejectsMovesFromCancelled();
    }
}
