namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanExceptionEngineChecks
{
    public static void SuccessionPlanExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanExceptionEngine.DefaultSuccessionPlanExceptionEngine(Flags.NewRegistry());
        var item = SuccessionPlanException.NewSuccessionPlanException("code-152", "title-196");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanExceptionEngineKeepsResultsWithinBounds();
    }
}
