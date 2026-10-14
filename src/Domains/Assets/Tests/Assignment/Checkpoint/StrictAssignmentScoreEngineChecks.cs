namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictAssignmentScoreEngineChecks
{
    public static void StrictAssignmentScoreEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAssignmentScoreEngine.DefaultStrictAssignmentScoreEngine(Flags.NewRegistry());
        var item = StrictAssignmentScore.NewStrictAssignmentScore("code-461", "name-530");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAssignmentScoreEngineKeepsResultsWithinBounds();
    }
}
