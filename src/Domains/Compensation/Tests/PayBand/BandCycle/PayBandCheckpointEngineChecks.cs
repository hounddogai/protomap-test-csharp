namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandCheckpointEngineChecks
{
    public static void PayBandCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandCheckpointEngine.DefaultPayBandCheckpointEngine(Flags.NewRegistry());
        var item = PayBandCheckpoint.NewPayBandCheckpoint("code-820", "name-806");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandCheckpointEngineKeepsResultsWithinBounds();
    }
}
