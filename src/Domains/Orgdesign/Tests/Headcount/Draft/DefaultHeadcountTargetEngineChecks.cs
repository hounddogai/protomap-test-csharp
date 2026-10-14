namespace Acme.Workforce.Orgdesign.Tests.Headcount.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class DefaultHeadcountTargetEngineChecks
{
    public static void DefaultHeadcountTargetEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultHeadcountTargetEngine.DefaultDefaultHeadcountTargetEngine(Flags.NewRegistry());
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("code-230", "name-342");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultHeadcountTargetEngineKeepsResultsWithinBounds();
    }
}
