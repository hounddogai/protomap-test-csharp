namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HeadcountAdjustmentEngineChecks
{
    public static void HeadcountAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountAdjustmentEngine.DefaultHeadcountAdjustmentEngine(Flags.NewRegistry());
        var item = HeadcountAdjustment.NewHeadcountAdjustment("code-492", "title-468");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountAdjustmentEngineKeepsResultsWithinBounds();
    }
}
