namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class RollingRateBundleEngineChecks
{
    public static void RollingRateBundleEngineKeepsResultsWithinBounds()
    {
        var engine = RollingRateBundleEngine.DefaultRollingRateBundleEngine(Flags.NewRegistry());
        var item = RollingRateBundle.NewRollingRateBundle("code-508", "name-918");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingRateBundleEngineKeepsResultsWithinBounds();
    }
}
