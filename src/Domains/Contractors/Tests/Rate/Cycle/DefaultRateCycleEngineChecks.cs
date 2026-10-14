namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultRateCycleEngineChecks
{
    public static void DefaultRateCycleEngineExtraKeepsResultsWithinBounds()
    {
        var engine = DefaultRateCycleEngine.DefaultDefaultRateCycleEngine(Flags.NewRegistry());
        var item = DefaultRateCycle.NewDefaultRateCycle("code-932", "title-765");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultRateCycleEngineExtraKeepsResultsWithinBounds();
    }
}
