namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ReturnPlanDigestEngineChecks
{
    public static void ReturnPlanDigestEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanDigestEngine.DefaultReturnPlanDigestEngine(Flags.NewRegistry());
        var item = ReturnPlanDigest.NewReturnPlanDigest("code-614", "name-754");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanDigestEngineKeepsResultsWithinBounds();
    }
}
