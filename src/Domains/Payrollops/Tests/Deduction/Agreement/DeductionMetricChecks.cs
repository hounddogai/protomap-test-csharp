namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionMetricChecks
{
    public static void DeductionMetricStartsInDraft()
    {
        var item = DeductionMetric.NewDeductionMetric("code-994", "name-342");
        Check.Equal(item.Status, DeductionMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionMetricMovesToSuspended()
    {
        var item = DeductionMetric.NewDeductionMetric("code-994", "name-342");
        try
        {
            item.MoveTo(DeductionMetricStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionMetricStatus.Suspended, "status");
    }

    public static void DeductionMetricRejectsMovesFromClosed()
    {
        var item = DeductionMetric.NewDeductionMetric("code-994", "name-342");
        item.Status = DeductionMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(DeductionMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DeductionMetricStartsInDraft();
        DeductionMetricMovesToSuspended();
        DeductionMetricRejectsMovesFromClosed();
    }
}
