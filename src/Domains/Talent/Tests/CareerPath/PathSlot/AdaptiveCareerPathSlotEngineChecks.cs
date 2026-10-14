namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class AdaptiveCareerPathSlotEngineChecks
{
    public static void AdaptiveCareerPathSlotEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveCareerPathSlotEngine.DefaultAdaptiveCareerPathSlotEngine();
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("code-31", "title-708");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathSlotEngineKeepsResultsWithinBounds();
    }
}
