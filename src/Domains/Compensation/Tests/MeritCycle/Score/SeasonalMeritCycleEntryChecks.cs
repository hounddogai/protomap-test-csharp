namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalMeritCycleEntryChecks
{
    public static void SeasonalMeritCycleEntryStartsInNew()
    {
        var item = SeasonalMeritCycleEntry.NewSeasonalMeritCycleEntry("code-217", "label-366");
        Check.Equal(item.Status, SeasonalMeritCycleEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalMeritCycleEntryMovesToQueued()
    {
        var item = SeasonalMeritCycleEntry.NewSeasonalMeritCycleEntry("code-217", "label-366");
        try
        {
            item.MoveTo(SeasonalMeritCycleEntryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalMeritCycleEntryStatus.Queued, "status");
    }

    public static void SeasonalMeritCycleEntryRejectsMovesFromFailed()
    {
        var item = SeasonalMeritCycleEntry.NewSeasonalMeritCycleEntry("code-217", "label-366");
        item.Status = SeasonalMeritCycleEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalMeritCycleEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalMeritCycleEntryStartsInNew();
        SeasonalMeritCycleEntryMovesToQueued();
        SeasonalMeritCycleEntryRejectsMovesFromFailed();
    }
}
