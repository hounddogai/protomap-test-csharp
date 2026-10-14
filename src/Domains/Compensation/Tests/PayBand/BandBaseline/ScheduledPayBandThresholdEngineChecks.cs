namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class ScheduledPayBandThresholdEngineChecks
{
    public static void ScheduledPayBandThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPayBandThresholdEngine.DefaultScheduledPayBandThresholdEngine(Flags.NewRegistry());
        var item = ScheduledPayBandThreshold.NewScheduledPayBandThreshold("code-94", "title-658");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPayBandThresholdEngineKeepsResultsWithinBounds();
    }
}
