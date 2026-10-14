namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientReturnPlanOverrideEngineChecks
{
    public static void LenientReturnPlanOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = LenientReturnPlanOverrideEngine.DefaultLenientReturnPlanOverrideEngine();
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("code-554", "name-821");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientReturnPlanOverrideEngineKeepsResultsWithinBounds();
    }
}
