namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentScoreEngineChecks
{
    public static void AssignmentScoreEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentScoreEngine.DefaultAssignmentScoreEngine();
        var item = AssignmentScore.NewAssignmentScore("code-661", "title-112");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentScoreEngineKeepsResultsWithinBounds();
    }
}
