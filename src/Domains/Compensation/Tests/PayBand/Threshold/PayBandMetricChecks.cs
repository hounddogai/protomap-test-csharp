namespace Acme.Workforce.Compensation.Tests.PayBand.Threshold;

using Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayBandMetricChecks
{
    public static void PayBandMetricStartsInPlanned()
    {
        var item = PayBandMetric.NewPayBandMetric("code-607", "title-840");
        Check.Equal(item.Status, PayBandMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandMetricMovesToCancelled()
    {
        var item = PayBandMetric.NewPayBandMetric("code-607", "title-840");
        try
        {
            item.MoveTo(PayBandMetricStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandMetricStatus.Cancelled, "status");
    }

    public static void PayBandMetricRejectsMovesFromCancelled()
    {
        var item = PayBandMetric.NewPayBandMetric("code-607", "title-840");
        item.Status = PayBandMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PayBandMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PayBandMetricStartsInPlanned();
        PayBandMetricMovesToCancelled();
        PayBandMetricRejectsMovesFromCancelled();
    }
}
