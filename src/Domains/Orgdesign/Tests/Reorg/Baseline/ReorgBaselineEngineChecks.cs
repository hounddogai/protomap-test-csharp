namespace Acme.Workforce.Orgdesign.Tests.Reorg.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ReorgBaselineEngineChecks
{
    public static void ReorgBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgBaselineEngine.DefaultReorgBaselineEngine(Flags.NewRegistry());
        var item = ReorgBaseline.NewReorgBaseline("code-977", "title-547");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgBaselineEngineKeepsResultsWithinBounds();
    }
}
