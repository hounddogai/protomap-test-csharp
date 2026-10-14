namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StrictHeadcountVarianceEngineChecks
{
    public static void StrictHeadcountVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = StrictHeadcountVarianceEngine.DefaultStrictHeadcountVarianceEngine(Flags.NewRegistry());
        var item = StrictHeadcountVariance.NewStrictHeadcountVariance("code-4", "label-193");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictHeadcountVarianceEngineKeepsResultsWithinBounds();
    }
}
