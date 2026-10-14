namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalTownHallGroupChecks
{
    public static void SeasonalTownHallGroupStartsInNew()
    {
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("code-574", "title-979");
        Check.Equal(item.Status, SeasonalTownHallGroupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalTownHallGroupMovesToQueued()
    {
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("code-574", "title-979");
        try
        {
            item.MoveTo(SeasonalTownHallGroupStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalTownHallGroupStatus.Queued, "status");
    }

    public static void SeasonalTownHallGroupRejectsMovesFromFailed()
    {
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("code-574", "title-979");
        item.Status = SeasonalTownHallGroupStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalTownHallGroupStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalTownHallGroupStartsInNew();
        SeasonalTownHallGroupMovesToQueued();
        SeasonalTownHallGroupRejectsMovesFromFailed();
    }
}
