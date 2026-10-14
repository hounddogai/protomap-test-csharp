namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Checks of pool batch code, run by the project's runner.</summary>
public static class ScheduledBonusPoolMilestoneEngineChecks
{
    public static void ScheduledBonusPoolMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledBonusPoolMilestoneEngine.DefaultScheduledBonusPoolMilestoneEngine();
        var item = ScheduledBonusPoolMilestone.NewScheduledBonusPoolMilestone("code-314", "name-205");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledBonusPoolMilestoneEngineKeepsResultsWithinBounds();
    }
}
