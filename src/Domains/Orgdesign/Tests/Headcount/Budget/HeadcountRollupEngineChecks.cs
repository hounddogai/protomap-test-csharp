namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HeadcountRollupEngineChecks
{
    public static void HeadcountRollupEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountRollupEngine.DefaultHeadcountRollupEngine(Flags.NewRegistry());
        var item = HeadcountRollup.NewHeadcountRollup("code-970", "label-400");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountRollupEngineKeepsResultsWithinBounds();
    }
}
