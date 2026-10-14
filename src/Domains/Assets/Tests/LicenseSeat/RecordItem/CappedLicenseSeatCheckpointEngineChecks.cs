namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedLicenseSeatCheckpointEngineChecks
{
    public static void CappedLicenseSeatCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = CappedLicenseSeatCheckpointEngine.DefaultCappedLicenseSeatCheckpointEngine(Flags.NewRegistry());
        var item = CappedLicenseSeatCheckpoint.NewCappedLicenseSeatCheckpoint("code-948", "title-530");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedLicenseSeatCheckpointEngineKeepsResultsWithinBounds();
    }
}
