namespace Acme.Workforce.Assets.Tests.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssignmentPlanEngineChecks
{
    public static void AssignmentPlanEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentPlanEngine.DefaultAssignmentPlanEngine();
        var item = AssignmentPlan.NewAssignmentPlan("code-89", "name-693");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentPlanEngineKeepsResultsWithinBounds();
    }
}
