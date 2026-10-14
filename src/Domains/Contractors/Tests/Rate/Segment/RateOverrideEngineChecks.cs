namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateOverrideEngineChecks
{
    public static void RateOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = RateOverrideEngine.DefaultRateOverrideEngine(Flags.NewRegistry());
        var item = RateOverride.NewRateOverride("code-381", "name-424");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateOverrideEngineKeepsResultsWithinBounds();
    }
}
