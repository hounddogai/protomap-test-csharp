namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReorgSummaryEngineChecks
{
    public static void ReorgSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgSummaryEngine.DefaultReorgSummaryEngine(Flags.NewRegistry());
        var item = ReorgSummary.NewReorgSummary("code-647", "label-612");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgSummaryEngineKeepsResultsWithinBounds();
    }
}
