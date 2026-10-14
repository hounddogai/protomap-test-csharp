namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class RateBaselineEngineChecks
{
    public static void RateBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = RateBaselineEngine.DefaultRateBaselineEngine(Flags.NewRegistry());
        var item = RateBaseline.NewRateBaseline("code-410", "name-822");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateBaselineEngineKeepsResultsWithinBounds();
    }
}
