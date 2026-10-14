namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class StrictEquityGrantMetricChecks
{
    public static void StrictEquityGrantMetricStartsInRequested()
    {
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("code-840", "title-14");
        Check.Equal(item.Status, StrictEquityGrantMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictEquityGrantMetricMovesToGranted()
    {
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("code-840", "title-14");
        try
        {
            item.MoveTo(StrictEquityGrantMetricStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictEquityGrantMetricStatus.Granted, "status");
    }

    public static void StrictEquityGrantMetricRejectsMovesFromExpired()
    {
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("code-840", "title-14");
        item.Status = StrictEquityGrantMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictEquityGrantMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictEquityGrantMetricStartsInRequested();
        StrictEquityGrantMetricMovesToGranted();
        StrictEquityGrantMetricRejectsMovesFromExpired();
    }
}
