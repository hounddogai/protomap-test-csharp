namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class SeasonalTownHallMetricChecks
{
    public static void SeasonalTownHallMetricStartsInCreated()
    {
        var item = SeasonalTownHallMetric.NewSeasonalTownHallMetric("code-396", "name-505");
        Check.Equal(item.Status, SeasonalTownHallMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalTownHallMetricMovesToRetired()
    {
        var item = SeasonalTownHallMetric.NewSeasonalTownHallMetric("code-396", "name-505");
        try
        {
            item.MoveTo(SeasonalTownHallMetricStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalTownHallMetricStatus.Retired, "status");
    }

    public static void SeasonalTownHallMetricRejectsMovesFromRetired()
    {
        var item = SeasonalTownHallMetric.NewSeasonalTownHallMetric("code-396", "name-505");
        item.Status = SeasonalTownHallMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalTownHallMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalTownHallMetricStartsInCreated();
        SeasonalTownHallMetricMovesToRetired();
        SeasonalTownHallMetricRejectsMovesFromRetired();
    }
}
