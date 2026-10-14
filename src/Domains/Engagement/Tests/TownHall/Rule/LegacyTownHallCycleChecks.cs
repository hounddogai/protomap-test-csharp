namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyTownHallCycleChecks
{
    public static void LegacyTownHallCycleStartsInOpen()
    {
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("code-873", "title-319");
        Check.Equal(item.Status, LegacyTownHallCycleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTownHallCycleMovesToInReview()
    {
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("code-873", "title-319");
        try
        {
            item.MoveTo(LegacyTownHallCycleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTownHallCycleStatus.InReview, "status");
    }

    public static void LegacyTownHallCycleRejectsMovesFromArchived()
    {
        var item = LegacyTownHallCycle.NewLegacyTownHallCycle("code-873", "title-319");
        item.Status = LegacyTownHallCycleStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyTownHallCycleStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyTownHallCycleStartsInOpen();
        LegacyTownHallCycleMovesToInReview();
        LegacyTownHallCycleRejectsMovesFromArchived();
    }
}
