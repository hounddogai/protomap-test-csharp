namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalPoolTierEngineChecks
{
    public static void SeasonalPoolTierEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPoolTierEngine.DefaultSeasonalPoolTierEngine(Flags.NewRegistry());
        var item = SeasonalPoolTier.NewSeasonalPoolTier("code-680", "name-9");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPoolTierEngineKeepsResultsWithinBounds();
    }
}
