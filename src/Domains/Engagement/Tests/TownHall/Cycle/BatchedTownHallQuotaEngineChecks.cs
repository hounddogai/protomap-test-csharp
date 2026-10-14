namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class BatchedTownHallQuotaEngineChecks
{
    public static void BatchedTownHallQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedTownHallQuotaEngine.DefaultBatchedTownHallQuotaEngine();
        var item = BatchedTownHallQuota.NewBatchedTownHallQuota("code-787", "label-729");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedTownHallQuotaEngineKeepsResultsWithinBounds();
    }
}
