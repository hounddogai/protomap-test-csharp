namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalTownHallCycleChecks
{
    public static void SeasonalTownHallCycleStartsInRequested()
    {
        var item = SeasonalTownHallCycle.NewSeasonalTownHallCycle("code-321", "label-761");
        Check.Equal(item.Status, SeasonalTownHallCycleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalTownHallCycleMovesToGranted()
    {
        var item = SeasonalTownHallCycle.NewSeasonalTownHallCycle("code-321", "label-761");
        try
        {
            item.MoveTo(SeasonalTownHallCycleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalTownHallCycleStatus.Granted, "status");
    }

    public static void SeasonalTownHallCycleRejectsMovesFromExpired()
    {
        var item = SeasonalTownHallCycle.NewSeasonalTownHallCycle("code-321", "label-761");
        item.Status = SeasonalTownHallCycleStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalTownHallCycleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalTownHallCycleStartsInRequested();
        SeasonalTownHallCycleMovesToGranted();
        SeasonalTownHallCycleRejectsMovesFromExpired();
    }
}
