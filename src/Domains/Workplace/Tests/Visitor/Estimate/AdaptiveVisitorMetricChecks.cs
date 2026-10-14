namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveVisitorMetricChecks
{
    public static void AdaptiveVisitorMetricStartsInRequested()
    {
        var item = AdaptiveVisitorMetric.NewAdaptiveVisitorMetric("code-41", "title-426");
        Check.Equal(item.Status, AdaptiveVisitorMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveVisitorMetricMovesToRevoked()
    {
        var item = AdaptiveVisitorMetric.NewAdaptiveVisitorMetric("code-41", "title-426");
        try
        {
            item.MoveTo(AdaptiveVisitorMetricStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveVisitorMetricStatus.Revoked, "status");
    }

    public static void AdaptiveVisitorMetricRejectsMovesFromExpired()
    {
        var item = AdaptiveVisitorMetric.NewAdaptiveVisitorMetric("code-41", "title-426");
        item.Status = AdaptiveVisitorMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveVisitorMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveVisitorMetricStartsInRequested();
        AdaptiveVisitorMetricMovesToRevoked();
        AdaptiveVisitorMetricRejectsMovesFromExpired();
    }
}
