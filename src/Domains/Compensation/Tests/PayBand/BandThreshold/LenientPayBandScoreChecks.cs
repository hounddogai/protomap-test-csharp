namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class LenientPayBandScoreChecks
{
    public static void LenientPayBandScoreStartsInDraft()
    {
        var item = LenientPayBandScore.NewLenientPayBandScore("code-467", "label-779");
        Check.Equal(item.Status, LenientPayBandScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPayBandScoreMovesToPending()
    {
        var item = LenientPayBandScore.NewLenientPayBandScore("code-467", "label-779");
        try
        {
            item.MoveTo(LenientPayBandScoreStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPayBandScoreStatus.Pending, "status");
    }

    public static void LenientPayBandScoreRejectsMovesFromClosed()
    {
        var item = LenientPayBandScore.NewLenientPayBandScore("code-467", "label-779");
        item.Status = LenientPayBandScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientPayBandScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientPayBandScoreStartsInDraft();
        LenientPayBandScoreMovesToPending();
        LenientPayBandScoreRejectsMovesFromClosed();
    }
}
