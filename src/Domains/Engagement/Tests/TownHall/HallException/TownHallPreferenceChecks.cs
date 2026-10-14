namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallPreferenceChecks
{
    public static void TownHallPreferenceStartsInNew()
    {
        var item = TownHallPreference.NewTownHallPreference("code-388", "label-781");
        Check.Equal(item.Status, TownHallPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallPreferenceMovesToQueued()
    {
        var item = TownHallPreference.NewTownHallPreference("code-388", "label-781");
        try
        {
            item.MoveTo(TownHallPreferenceStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallPreferenceStatus.Queued, "status");
    }

    public static void TownHallPreferenceRejectsMovesFromFailed()
    {
        var item = TownHallPreference.NewTownHallPreference("code-388", "label-781");
        item.Status = TownHallPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(TownHallPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        TownHallPreferenceStartsInNew();
        TownHallPreferenceMovesToQueued();
        TownHallPreferenceRejectsMovesFromFailed();
    }
}
