namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ScheduledTaxFormMetricChecks
{
    public static void ScheduledTaxFormMetricStartsInPlanned()
    {
        var item = ScheduledTaxFormMetric.NewScheduledTaxFormMetric("code-506", "name-287");
        Check.Equal(item.Status, ScheduledTaxFormMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledTaxFormMetricMovesToInProgress()
    {
        var item = ScheduledTaxFormMetric.NewScheduledTaxFormMetric("code-506", "name-287");
        try
        {
            item.MoveTo(ScheduledTaxFormMetricStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledTaxFormMetricStatus.InProgress, "status");
    }

    public static void ScheduledTaxFormMetricRejectsMovesFromCancelled()
    {
        var item = ScheduledTaxFormMetric.NewScheduledTaxFormMetric("code-506", "name-287");
        item.Status = ScheduledTaxFormMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledTaxFormMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledTaxFormMetricStartsInPlanned();
        ScheduledTaxFormMetricMovesToInProgress();
        ScheduledTaxFormMetricRejectsMovesFromCancelled();
    }
}
