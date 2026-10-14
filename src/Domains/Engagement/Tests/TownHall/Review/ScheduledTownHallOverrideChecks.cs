namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledTownHallOverrideChecks
{
    public static void ScheduledTownHallOverrideStartsInOpen()
    {
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("code-988", "title-93");
        Check.Equal(item.Status, ScheduledTownHallOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledTownHallOverrideMovesToArchived()
    {
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("code-988", "title-93");
        try
        {
            item.MoveTo(ScheduledTownHallOverrideStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledTownHallOverrideStatus.Archived, "status");
    }

    public static void ScheduledTownHallOverrideRejectsMovesFromArchived()
    {
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("code-988", "title-93");
        item.Status = ScheduledTownHallOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledTownHallOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledTownHallOverrideStartsInOpen();
        ScheduledTownHallOverrideMovesToArchived();
        ScheduledTownHallOverrideRejectsMovesFromArchived();
    }
}
