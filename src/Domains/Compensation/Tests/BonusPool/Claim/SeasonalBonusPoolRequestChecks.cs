namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SeasonalBonusPoolRequestChecks
{
    public static void SeasonalBonusPoolRequestStartsInNew()
    {
        var item = SeasonalBonusPoolRequest.NewSeasonalBonusPoolRequest("code-908", "label-272");
        Check.Equal(item.Status, SeasonalBonusPoolRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalBonusPoolRequestMovesToDone()
    {
        var item = SeasonalBonusPoolRequest.NewSeasonalBonusPoolRequest("code-908", "label-272");
        try
        {
            item.MoveTo(SeasonalBonusPoolRequestStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalBonusPoolRequestStatus.Done, "status");
    }

    public static void SeasonalBonusPoolRequestRejectsMovesFromFailed()
    {
        var item = SeasonalBonusPoolRequest.NewSeasonalBonusPoolRequest("code-908", "label-272");
        item.Status = SeasonalBonusPoolRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalBonusPoolRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalBonusPoolRequestStartsInNew();
        SeasonalBonusPoolRequestMovesToDone();
        SeasonalBonusPoolRequestRejectsMovesFromFailed();
    }
}
