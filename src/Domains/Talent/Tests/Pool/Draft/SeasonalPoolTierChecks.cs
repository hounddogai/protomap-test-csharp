namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalPoolTierChecks
{
    public static void SeasonalPoolTierStartsInRequested()
    {
        var item = SeasonalPoolTier.NewSeasonalPoolTier("code-680", "name-9");
        Check.Equal(item.Status, SeasonalPoolTierStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPoolTierMovesToGranted()
    {
        var item = SeasonalPoolTier.NewSeasonalPoolTier("code-680", "name-9");
        try
        {
            item.MoveTo(SeasonalPoolTierStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPoolTierStatus.Granted, "status");
    }

    public static void SeasonalPoolTierRejectsMovesFromExpired()
    {
        var item = SeasonalPoolTier.NewSeasonalPoolTier("code-680", "name-9");
        item.Status = SeasonalPoolTierStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalPoolTierStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalPoolTierStartsInRequested();
        SeasonalPoolTierMovesToGranted();
        SeasonalPoolTierRejectsMovesFromExpired();
    }
}
