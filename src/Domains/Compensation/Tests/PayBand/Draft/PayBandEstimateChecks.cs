namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandEstimateChecks
{
    public static void PayBandEstimateStartsInNew()
    {
        var item = PayBandEstimate.NewPayBandEstimate("code-610", "label-938");
        Check.Equal(item.Status, PayBandEstimateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandEstimateMovesToRunning()
    {
        var item = PayBandEstimate.NewPayBandEstimate("code-610", "label-938");
        try
        {
            item.MoveTo(PayBandEstimateStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandEstimateStatus.Running, "status");
    }

    public static void PayBandEstimateRejectsMovesFromFailed()
    {
        var item = PayBandEstimate.NewPayBandEstimate("code-610", "label-938");
        item.Status = PayBandEstimateStatus.Failed;
        Check.Fails(() => item.MoveTo(PayBandEstimateStatus.New), "move");
    }

    public static void RunAll()
    {
        PayBandEstimateStartsInNew();
        PayBandEstimateMovesToRunning();
        PayBandEstimateRejectsMovesFromFailed();
    }
}
