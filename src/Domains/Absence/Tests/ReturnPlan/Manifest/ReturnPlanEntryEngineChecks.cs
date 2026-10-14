namespace Acme.Workforce.Absence.Tests.ReturnPlan.Manifest;

using Acme.Workforce.Absence.ReturnPlan.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ReturnPlanEntryEngineChecks
{
    public static void ReturnPlanEntryEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanEntryEngine.DefaultReturnPlanEntryEngine();
        var item = ReturnPlanEntry.NewReturnPlanEntry("code-160", "title-60");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanEntryEngineKeepsResultsWithinBounds();
    }
}
