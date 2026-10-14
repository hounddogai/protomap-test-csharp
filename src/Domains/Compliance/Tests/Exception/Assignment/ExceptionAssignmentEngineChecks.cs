namespace Acme.Workforce.Compliance.Tests.Exception.Assignment;

using Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ExceptionAssignmentEngineChecks
{
    public static void ExceptionAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionAssignmentEngine.DefaultExceptionAssignmentEngine();
        var item = ExceptionAssignment.NewExceptionAssignment("code-347", "title-47");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionAssignmentEngineKeepsResultsWithinBounds();
    }
}
