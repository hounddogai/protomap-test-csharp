namespace Acme.Workforce.Compensation.Tests.PayBand.Cycle;

using Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TieredPayBandScoreChecks
{
    public static void TieredPayBandScoreStartsInDraft()
    {
        var item = TieredPayBandScore.NewTieredPayBandScore("code-986", "title-910");
        Check.Equal(item.Status, TieredPayBandScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPayBandScoreMovesToClosed()
    {
        var item = TieredPayBandScore.NewTieredPayBandScore("code-986", "title-910");
        try
        {
            item.MoveTo(TieredPayBandScoreStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPayBandScoreStatus.Closed, "status");
    }

    public static void TieredPayBandScoreRejectsMovesFromClosed()
    {
        var item = TieredPayBandScore.NewTieredPayBandScore("code-986", "title-910");
        item.Status = TieredPayBandScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredPayBandScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredPayBandScoreStartsInDraft();
        TieredPayBandScoreMovesToClosed();
        TieredPayBandScoreRejectsMovesFromClosed();
    }
}
