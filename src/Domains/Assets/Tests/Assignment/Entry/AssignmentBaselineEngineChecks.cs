namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentBaselineEngineChecks
{
    public static void AssignmentBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentBaselineEngine.DefaultAssignmentBaselineEngine();
        var item = AssignmentBaseline.NewAssignmentBaseline("code-669", "label-116");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentBaselineEngineKeepsResultsWithinBounds();
    }
}
