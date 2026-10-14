namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorPlanEngineChecks
{
    public static void VisitorPlanEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorPlanEngine.DefaultVisitorPlanEngine();
        var item = VisitorPlan.NewVisitorPlan("code-480", "label-840");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorPlanEngineKeepsResultsWithinBounds();
    }
}
