namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPayBandEstimateChecks
{
    public static void TieredPayBandEstimateStartsInIdle()
    {
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("code-627", "label-771");
        Check.Equal(item.Status, TieredPayBandEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPayBandEstimateMovesToReady()
    {
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("code-627", "label-771");
        try
        {
            item.MoveTo(TieredPayBandEstimateStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPayBandEstimateStatus.Ready, "status");
    }

    public static void TieredPayBandEstimateRejectsMovesFromStopped()
    {
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("code-627", "label-771");
        item.Status = TieredPayBandEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredPayBandEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredPayBandEstimateStartsInIdle();
        TieredPayBandEstimateMovesToReady();
        TieredPayBandEstimateRejectsMovesFromStopped();
    }
}
