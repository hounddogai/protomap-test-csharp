namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalTownHallClaimChecks
{
    public static void SeasonalTownHallClaimStartsInNew()
    {
        var item = SeasonalTownHallClaim.NewSeasonalTownHallClaim("code-574", "label-903");
        Check.Equal(item.Status, SeasonalTownHallClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalTownHallClaimMovesToQueued()
    {
        var item = SeasonalTownHallClaim.NewSeasonalTownHallClaim("code-574", "label-903");
        try
        {
            item.MoveTo(SeasonalTownHallClaimStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalTownHallClaimStatus.Queued, "status");
    }

    public static void SeasonalTownHallClaimRejectsMovesFromFailed()
    {
        var item = SeasonalTownHallClaim.NewSeasonalTownHallClaim("code-574", "label-903");
        item.Status = SeasonalTownHallClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalTownHallClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalTownHallClaimStartsInNew();
        SeasonalTownHallClaimMovesToQueued();
        SeasonalTownHallClaimRejectsMovesFromFailed();
    }
}
