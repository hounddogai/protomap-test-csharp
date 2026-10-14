namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class HeadcountTargetEngineChecks
{
    public static void HeadcountTargetEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountTargetEngine.DefaultHeadcountTargetEngine(Flags.NewRegistry());
        var item = HeadcountTarget.NewHeadcountTarget("code-22", "label-480");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountTargetEngineKeepsResultsWithinBounds();
    }
}
