namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionCheckpointEngineChecks
{
    public static void ExceptionCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionCheckpointEngine.DefaultExceptionCheckpointEngine(Flags.NewRegistry());
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("code-328", "label-568");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionCheckpointEngineKeepsResultsWithinBounds();
    }
}
