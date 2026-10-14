namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementMetricChecks
{
    public static void EngagementMetricStartsInProposed()
    {
        var item = EngagementMetric.NewEngagementMetric("code-858", "name-137");
        Check.Equal(item.Status, EngagementMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementMetricMovesToApproved()
    {
        var item = EngagementMetric.NewEngagementMetric("code-858", "name-137");
        try
        {
            item.MoveTo(EngagementMetricStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementMetricStatus.Approved, "status");
    }

    public static void EngagementMetricRejectsMovesFromWithdrawn()
    {
        var item = EngagementMetric.NewEngagementMetric("code-858", "name-137");
        item.Status = EngagementMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EngagementMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EngagementMetricStartsInProposed();
        EngagementMetricMovesToApproved();
        EngagementMetricRejectsMovesFromWithdrawn();
    }
}
