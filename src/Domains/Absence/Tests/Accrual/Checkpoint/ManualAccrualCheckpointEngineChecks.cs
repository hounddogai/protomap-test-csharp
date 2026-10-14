namespace Acme.Workforce.Absence.Tests.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ManualAccrualCheckpointEngineChecks
{
    public static void ManualAccrualCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = ManualAccrualCheckpointEngine.DefaultManualAccrualCheckpointEngine(Flags.NewRegistry());
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("code-11", "title-960");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualAccrualCheckpointEngineKeepsResultsWithinBounds();
    }
}
