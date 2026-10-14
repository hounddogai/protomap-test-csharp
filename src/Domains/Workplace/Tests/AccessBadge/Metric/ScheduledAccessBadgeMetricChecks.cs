namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledAccessBadgeMetricChecks
{
    public static void ScheduledAccessBadgeMetricStartsInProposed()
    {
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("code-863", "name-726");
        Check.Equal(item.Status, ScheduledAccessBadgeMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAccessBadgeMetricMovesToApproved()
    {
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("code-863", "name-726");
        try
        {
            item.MoveTo(ScheduledAccessBadgeMetricStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAccessBadgeMetricStatus.Approved, "status");
    }

    public static void ScheduledAccessBadgeMetricRejectsMovesFromWithdrawn()
    {
        var item = ScheduledAccessBadgeMetric.NewScheduledAccessBadgeMetric("code-863", "name-726");
        item.Status = ScheduledAccessBadgeMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledAccessBadgeMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledAccessBadgeMetricStartsInProposed();
        ScheduledAccessBadgeMetricMovesToApproved();
        ScheduledAccessBadgeMetricRejectsMovesFromWithdrawn();
    }
}
