namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPayBandSnapshotEngineChecks
{
    public static void ScheduledPayBandSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPayBandSnapshotEngine.DefaultScheduledPayBandSnapshotEngine();
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("code-547", "name-663");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPayBandSnapshotEngineKeepsResultsWithinBounds();
    }
}
