namespace Acme.Workforce.Absence.Tests.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveAccrualMetricChecks
{
    public static void AdaptiveAccrualMetricStartsInRequested()
    {
        var item = AdaptiveAccrualMetric.NewAdaptiveAccrualMetric("code-604", "title-11");
        Check.Equal(item.Status, AdaptiveAccrualMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAccrualMetricMovesToExpired()
    {
        var item = AdaptiveAccrualMetric.NewAdaptiveAccrualMetric("code-604", "title-11");
        try
        {
            item.MoveTo(AdaptiveAccrualMetricStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAccrualMetricStatus.Expired, "status");
    }

    public static void AdaptiveAccrualMetricRejectsMovesFromExpired()
    {
        var item = AdaptiveAccrualMetric.NewAdaptiveAccrualMetric("code-604", "title-11");
        item.Status = AdaptiveAccrualMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveAccrualMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveAccrualMetricStartsInRequested();
        AdaptiveAccrualMetricMovesToExpired();
        AdaptiveAccrualMetricRejectsMovesFromExpired();
    }
}
