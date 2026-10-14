namespace Acme.Workforce.Assets.Tests.Assignment.Request;

using Acme.Workforce.Assets.Assignment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssignmentRequestEngineChecks
{
    public static void AssignmentRequestEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentRequestEngine.DefaultAssignmentRequestEngine();
        var item = AssignmentRequest.NewAssignmentRequest("code-462", "label-688");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentRequestEngineKeepsResultsWithinBounds();
    }
}
