namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionPlanEngineChecks
{
    public static void ExceptionPlanEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionPlanEngine.DefaultExceptionPlanEngine();
        var item = ExceptionPlan.NewExceptionPlan("code-140", "label-622");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionPlanEngineKeepsResultsWithinBounds();
    }
}
