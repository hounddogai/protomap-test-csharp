namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientVisitorCheckpointEngineChecks
{
    public static void LenientVisitorCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = LenientVisitorCheckpointEngine.DefaultLenientVisitorCheckpointEngine(Flags.NewRegistry());
        var item = LenientVisitorCheckpoint.NewLenientVisitorCheckpoint("code-869", "name-199");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientVisitorCheckpointEngineKeepsResultsWithinBounds();
    }
}
