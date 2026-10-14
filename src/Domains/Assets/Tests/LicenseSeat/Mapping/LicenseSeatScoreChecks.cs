namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatScoreChecks
{
    public static void LicenseSeatScoreStartsInNew()
    {
        var item = LicenseSeatScore.NewLicenseSeatScore("code-365", "title-509");
        Check.Equal(item.Status, LicenseSeatScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatScoreMovesToQueued()
    {
        var item = LicenseSeatScore.NewLicenseSeatScore("code-365", "title-509");
        try
        {
            item.MoveTo(LicenseSeatScoreStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatScoreStatus.Queued, "status");
    }

    public static void LicenseSeatScoreRejectsMovesFromFailed()
    {
        var item = LicenseSeatScore.NewLicenseSeatScore("code-365", "title-509");
        item.Status = LicenseSeatScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(LicenseSeatScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        LicenseSeatScoreStartsInNew();
        LicenseSeatScoreMovesToQueued();
        LicenseSeatScoreRejectsMovesFromFailed();
    }
}
