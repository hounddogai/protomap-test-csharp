namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientSuccessionPlanRecordEngineChecks
{
    public static void LenientSuccessionPlanRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LenientSuccessionPlanRecordEngine.DefaultLenientSuccessionPlanRecordEngine(Flags.NewRegistry());
        var item = LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord("code-53", "name-23");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanRecordEngineKeepsResultsWithinBounds();
    }
}
