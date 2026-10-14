namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FallbackPayBandScoreChecks
{
    public static void FallbackPayBandScoreStartsInOpen()
    {
        var item = FallbackPayBandScore.NewFallbackPayBandScore("code-180", "name-235");
        Check.Equal(item.Status, FallbackPayBandScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayBandScoreMovesToInReview()
    {
        var item = FallbackPayBandScore.NewFallbackPayBandScore("code-180", "name-235");
        try
        {
            item.MoveTo(FallbackPayBandScoreStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayBandScoreStatus.InReview, "status");
    }

    public static void FallbackPayBandScoreRejectsMovesFromArchived()
    {
        var item = FallbackPayBandScore.NewFallbackPayBandScore("code-180", "name-235");
        item.Status = FallbackPayBandScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackPayBandScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackPayBandScoreStartsInOpen();
        FallbackPayBandScoreMovesToInReview();
        FallbackPayBandScoreRejectsMovesFromArchived();
    }
}
