namespace Acme.Workforce.Workplace.Tests.Occupancy.Metric;

using Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PrimaryOccupancyMetricChecks
{
    public static void PrimaryOccupancyMetricStartsInOpen()
    {
        var item = PrimaryOccupancyMetric.NewPrimaryOccupancyMetric("code-791", "label-803");
        Check.Equal(item.Status, PrimaryOccupancyMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryOccupancyMetricMovesToReopened()
    {
        var item = PrimaryOccupancyMetric.NewPrimaryOccupancyMetric("code-791", "label-803");
        try
        {
            item.MoveTo(PrimaryOccupancyMetricStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryOccupancyMetricStatus.Reopened, "status");
    }

    public static void PrimaryOccupancyMetricRejectsMovesFromArchived()
    {
        var item = PrimaryOccupancyMetric.NewPrimaryOccupancyMetric("code-791", "label-803");
        item.Status = PrimaryOccupancyMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryOccupancyMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryOccupancyMetricStartsInOpen();
        PrimaryOccupancyMetricMovesToReopened();
        PrimaryOccupancyMetricRejectsMovesFromArchived();
    }
}
