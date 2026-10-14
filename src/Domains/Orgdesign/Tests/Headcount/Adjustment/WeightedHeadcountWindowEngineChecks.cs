namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedHeadcountWindowEngineChecks
{
    public static void WeightedHeadcountWindowEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedHeadcountWindowEngine.DefaultWeightedHeadcountWindowEngine(Flags.NewRegistry());
        var item = WeightedHeadcountWindow.NewWeightedHeadcountWindow("code-729", "label-977");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedHeadcountWindowEngineKeepsResultsWithinBounds();
    }
}
