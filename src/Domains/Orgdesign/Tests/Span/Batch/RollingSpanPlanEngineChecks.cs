namespace Acme.Workforce.Orgdesign.Tests.Span.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RollingSpanPlanEngineChecks
{
    public static void RollingSpanPlanEngineKeepsResultsWithinBounds()
    {
        var engine = RollingSpanPlanEngine.DefaultRollingSpanPlanEngine(Flags.NewRegistry());
        var item = RollingSpanPlan.NewRollingSpanPlan("code-636", "name-94");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingSpanPlanEngineKeepsResultsWithinBounds();
    }
}
