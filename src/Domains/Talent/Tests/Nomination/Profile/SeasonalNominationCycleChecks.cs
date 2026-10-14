namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalNominationCycleChecks
{
    public static void SeasonalNominationCycleStartsInDraft()
    {
        var item = SeasonalNominationCycle.NewSeasonalNominationCycle("code-67", "name-857");
        Check.Equal(item.Status, SeasonalNominationCycleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalNominationCycleMovesToClosed()
    {
        var item = SeasonalNominationCycle.NewSeasonalNominationCycle("code-67", "name-857");
        try
        {
            item.MoveTo(SeasonalNominationCycleStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalNominationCycleStatus.Closed, "status");
    }

    public static void SeasonalNominationCycleRejectsMovesFromClosed()
    {
        var item = SeasonalNominationCycle.NewSeasonalNominationCycle("code-67", "name-857");
        item.Status = SeasonalNominationCycleStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalNominationCycleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalNominationCycleStartsInDraft();
        SeasonalNominationCycleMovesToClosed();
        SeasonalNominationCycleRejectsMovesFromClosed();
    }
}
