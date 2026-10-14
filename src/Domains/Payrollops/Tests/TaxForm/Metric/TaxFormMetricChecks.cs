namespace Acme.Workforce.Payrollops.Tests.TaxForm.Metric;

using Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TaxFormMetricChecks
{
    public static void TaxFormMetricStartsInIdle()
    {
        var item = TaxFormMetric.NewTaxFormMetric("code-252", "label-795");
        Check.Equal(item.Status, TaxFormMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormMetricMovesToStopped()
    {
        var item = TaxFormMetric.NewTaxFormMetric("code-252", "label-795");
        try
        {
            item.MoveTo(TaxFormMetricStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormMetricStatus.Stopped, "status");
    }

    public static void TaxFormMetricRejectsMovesFromStopped()
    {
        var item = TaxFormMetric.NewTaxFormMetric("code-252", "label-795");
        item.Status = TaxFormMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(TaxFormMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TaxFormMetricStartsInIdle();
        TaxFormMetricMovesToStopped();
        TaxFormMetricRejectsMovesFromStopped();
    }
}
