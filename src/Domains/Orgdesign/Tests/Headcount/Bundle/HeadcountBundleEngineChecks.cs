namespace Acme.Workforce.Orgdesign.Tests.Headcount.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class HeadcountBundleEngineChecks
{
    public static void HeadcountBundleEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountBundleEngine.DefaultHeadcountBundleEngine(Flags.NewRegistry());
        var item = HeadcountBundle.NewHeadcountBundle("code-460", "label-92");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountBundleEngineKeepsResultsWithinBounds();
    }
}
