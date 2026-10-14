namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ScheduledVisitorMetricChecks
{
    public static void ScheduledVisitorMetricStartsInOpen()
    {
        var item = ScheduledVisitorMetric.NewScheduledVisitorMetric("code-708", "name-961");
        Check.Equal(item.Status, ScheduledVisitorMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledVisitorMetricMovesToResolved()
    {
        var item = ScheduledVisitorMetric.NewScheduledVisitorMetric("code-708", "name-961");
        try
        {
            item.MoveTo(ScheduledVisitorMetricStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledVisitorMetricStatus.Resolved, "status");
    }

    public static void ScheduledVisitorMetricRejectsMovesFromArchived()
    {
        var item = ScheduledVisitorMetric.NewScheduledVisitorMetric("code-708", "name-961");
        item.Status = ScheduledVisitorMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledVisitorMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledVisitorMetricStartsInOpen();
        ScheduledVisitorMetricMovesToResolved();
        ScheduledVisitorMetricRejectsMovesFromArchived();
    }
}
