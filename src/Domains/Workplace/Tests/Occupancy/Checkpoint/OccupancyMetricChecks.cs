namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class OccupancyMetricChecks
{
    public static void OccupancyMetricStartsInOpen()
    {
        var item = OccupancyMetric.NewOccupancyMetric("code-361", "name-481");
        Check.Equal(item.Status, OccupancyMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyMetricMovesToArchived()
    {
        var item = OccupancyMetric.NewOccupancyMetric("code-361", "name-481");
        try
        {
            item.MoveTo(OccupancyMetricStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyMetricStatus.Archived, "status");
    }

    public static void OccupancyMetricRejectsMovesFromArchived()
    {
        var item = OccupancyMetric.NewOccupancyMetric("code-361", "name-481");
        item.Status = OccupancyMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(OccupancyMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        OccupancyMetricStartsInOpen();
        OccupancyMetricMovesToArchived();
        OccupancyMetricRejectsMovesFromArchived();
    }
}
