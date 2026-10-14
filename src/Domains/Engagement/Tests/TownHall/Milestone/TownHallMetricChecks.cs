namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallMetricChecks
{
    public static void TownHallMetricStartsInOpen()
    {
        var item = TownHallMetric.NewTownHallMetric("code-128", "title-815");
        Check.Equal(item.Status, TownHallMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallMetricMovesToInReview()
    {
        var item = TownHallMetric.NewTownHallMetric("code-128", "title-815");
        try
        {
            item.MoveTo(TownHallMetricStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallMetricStatus.InReview, "status");
    }

    public static void TownHallMetricRejectsMovesFromArchived()
    {
        var item = TownHallMetric.NewTownHallMetric("code-128", "title-815");
        item.Status = TownHallMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(TownHallMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        TownHallMetricStartsInOpen();
        TownHallMetricMovesToInReview();
        TownHallMetricRejectsMovesFromArchived();
    }
}
