namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandScoreChecks
{
    public static void PayBandScoreStartsInDraft()
    {
        var item = PayBandScore.NewPayBandScore("code-828", "label-850");
        Check.Equal(item.Status, PayBandScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandScoreMovesToActive()
    {
        var item = PayBandScore.NewPayBandScore("code-828", "label-850");
        try
        {
            item.MoveTo(PayBandScoreStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandScoreStatus.Active, "status");
    }

    public static void PayBandScoreRejectsMovesFromClosed()
    {
        var item = PayBandScore.NewPayBandScore("code-828", "label-850");
        item.Status = PayBandScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(PayBandScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PayBandScoreStartsInDraft();
        PayBandScoreMovesToActive();
        PayBandScoreRejectsMovesFromClosed();
    }
}
