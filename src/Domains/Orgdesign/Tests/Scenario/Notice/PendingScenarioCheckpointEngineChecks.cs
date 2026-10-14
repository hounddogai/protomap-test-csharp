namespace Acme.Workforce.Orgdesign.Tests.Scenario.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingScenarioCheckpointEngineChecks
{
    public static void PendingScenarioCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = PendingScenarioCheckpointEngine.DefaultPendingScenarioCheckpointEngine(Flags.NewRegistry());
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("code-286", "label-752");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingScenarioCheckpointEngineKeepsResultsWithinBounds();
    }
}
