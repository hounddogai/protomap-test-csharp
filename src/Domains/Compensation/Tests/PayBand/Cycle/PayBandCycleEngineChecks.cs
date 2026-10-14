namespace Acme.Workforce.Compensation.Tests.PayBand.Cycle;

using Acme.Workforce.Compensation.PayBand.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PayBandCycleEngineChecks
{
    public static void PayBandCycleEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandCycleEngine.DefaultPayBandCycleEngine(Flags.NewRegistry());
        var item = PayBandCycle.NewPayBandCycle("code-740", "name-954");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandCycleEngineKeepsResultsWithinBounds();
    }
}
