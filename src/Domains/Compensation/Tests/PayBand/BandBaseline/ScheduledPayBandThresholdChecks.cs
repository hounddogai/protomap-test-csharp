namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class ScheduledPayBandThresholdChecks
{
    public static void ScheduledPayBandThresholdStartsInIdle()
    {
        var item = ScheduledPayBandThreshold.NewScheduledPayBandThreshold("code-94", "title-658");
        Check.Equal(item.Status, ScheduledPayBandThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayBandThresholdMovesToReady()
    {
        var item = ScheduledPayBandThreshold.NewScheduledPayBandThreshold("code-94", "title-658");
        try
        {
            item.MoveTo(ScheduledPayBandThresholdStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayBandThresholdStatus.Ready, "status");
    }

    public static void ScheduledPayBandThresholdRejectsMovesFromStopped()
    {
        var item = ScheduledPayBandThreshold.NewScheduledPayBandThreshold("code-94", "title-658");
        item.Status = ScheduledPayBandThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledPayBandThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledPayBandThresholdStartsInIdle();
        ScheduledPayBandThresholdMovesToReady();
        ScheduledPayBandThresholdRejectsMovesFromStopped();
    }
}
