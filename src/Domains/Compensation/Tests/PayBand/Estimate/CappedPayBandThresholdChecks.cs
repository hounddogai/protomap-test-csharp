namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class CappedPayBandThresholdChecks
{
    public static void CappedPayBandThresholdStartsInIdle()
    {
        var item = CappedPayBandThreshold.NewCappedPayBandThreshold("code-343", "label-105");
        Check.Equal(item.Status, CappedPayBandThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPayBandThresholdMovesToReady()
    {
        var item = CappedPayBandThreshold.NewCappedPayBandThreshold("code-343", "label-105");
        try
        {
            item.MoveTo(CappedPayBandThresholdStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPayBandThresholdStatus.Ready, "status");
    }

    public static void CappedPayBandThresholdRejectsMovesFromStopped()
    {
        var item = CappedPayBandThreshold.NewCappedPayBandThreshold("code-343", "label-105");
        item.Status = CappedPayBandThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedPayBandThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedPayBandThresholdStartsInIdle();
        CappedPayBandThresholdMovesToReady();
        CappedPayBandThresholdRejectsMovesFromStopped();
    }
}
