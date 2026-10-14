namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionMilestoneEngineChecks
{
    public static void ExceptionMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionMilestoneEngine.DefaultExceptionMilestoneEngine();
        var item = ExceptionMilestone.NewExceptionMilestone("code-607", "title-283");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionMilestoneEngineKeepsResultsWithinBounds();
    }
}
