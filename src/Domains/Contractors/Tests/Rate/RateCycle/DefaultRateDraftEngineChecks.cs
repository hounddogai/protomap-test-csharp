namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class DefaultRateDraftEngineChecks
{
    public static void DefaultRateDraftEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultRateDraftEngine.DefaultDefaultRateDraftEngine(Flags.NewRegistry());
        var item = DefaultRateDraft.NewDefaultRateDraft("code-837", "name-648");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultRateDraftEngineKeepsResultsWithinBounds();
    }
}
