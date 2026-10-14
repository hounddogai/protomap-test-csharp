namespace Acme.Workforce.Contractors.Tests.Rate.Notice;

using Acme.Workforce.Contractors.Rate.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RatePolicyEngineChecks
{
    public static void RatePolicyEngineKeepsResultsWithinBounds()
    {
        var engine = RatePolicyEngine.DefaultRatePolicyEngine(Flags.NewRegistry());
        var item = RatePolicy.NewRatePolicy("code-79", "label-446");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RatePolicyEngineKeepsResultsWithinBounds();
    }
}
