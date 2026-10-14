namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalExceptionCheckpointEngineChecks
{
    public static void RegionalExceptionCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalExceptionCheckpointEngine.DefaultRegionalExceptionCheckpointEngine(Flags.NewRegistry());
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("code-91", "title-487");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalExceptionCheckpointEngineKeepsResultsWithinBounds();
    }
}
