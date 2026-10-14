namespace Acme.Workforce.Payrollops.Tests.PayRun.Metric;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PayRunMetricChecks
{
    public static void PayRunMetricStartsInProposed()
    {
        var item = PayRunMetric.NewPayRunMetric("code-130", "title-81");
        Check.Equal(item.Status, PayRunMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunMetricMovesToRejected()
    {
        var item = PayRunMetric.NewPayRunMetric("code-130", "title-81");
        try
        {
            item.MoveTo(PayRunMetricStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunMetricStatus.Rejected, "status");
    }

    public static void PayRunMetricRejectsMovesFromWithdrawn()
    {
        var item = PayRunMetric.NewPayRunMetric("code-130", "title-81");
        item.Status = PayRunMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayRunMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayRunMetricStartsInProposed();
        PayRunMetricMovesToRejected();
        PayRunMetricRejectsMovesFromWithdrawn();
    }
}
