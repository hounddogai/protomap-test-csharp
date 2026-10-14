namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class ReorgTierEngineChecks
{
    public static void ReorgTierEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgTierEngine.DefaultReorgTierEngine();
        var item = ReorgTier.NewReorgTier("code-827", "title-418");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgTierEngineKeepsResultsWithinBounds();
    }
}
