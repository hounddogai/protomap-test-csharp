namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountCheckpointEngineChecks
{
    public static void HeadcountCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountCheckpointEngine.DefaultHeadcountCheckpointEngine();
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("code-459", "name-34");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountCheckpointEngineKeepsResultsWithinBounds();
    }
}
