namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictMobilityOverrideEngineChecks
{
    public static void StrictMobilityOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = StrictMobilityOverrideEngine.DefaultStrictMobilityOverrideEngine(Flags.NewRegistry());
        var item = StrictMobilityOverride.NewStrictMobilityOverride("code-244", "title-456");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictMobilityOverrideEngineKeepsResultsWithinBounds();
    }
}
