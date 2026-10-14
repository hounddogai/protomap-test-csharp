namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeMetricChecks
{
    public static void AccessBadgeMetricStartsInDraft()
    {
        var item = AccessBadgeMetric.NewAccessBadgeMetric("code-173", "label-849");
        Check.Equal(item.Status, AccessBadgeMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeMetricMovesToActive()
    {
        var item = AccessBadgeMetric.NewAccessBadgeMetric("code-173", "label-849");
        try
        {
            item.MoveTo(AccessBadgeMetricStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeMetricStatus.Active, "status");
    }

    public static void AccessBadgeMetricRejectsMovesFromClosed()
    {
        var item = AccessBadgeMetric.NewAccessBadgeMetric("code-173", "label-849");
        item.Status = AccessBadgeMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgeMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgeMetricStartsInDraft();
        AccessBadgeMetricMovesToActive();
        AccessBadgeMetricRejectsMovesFromClosed();
    }
}
