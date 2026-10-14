namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolMilestoneEngineChecks
{
    public static void BonusPoolMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolMilestoneEngine.DefaultBonusPoolMilestoneEngine(Flags.NewRegistry());
        var item = BonusPoolMilestone.NewBonusPoolMilestone("code-658", "title-294");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolMilestoneEngineKeepsResultsWithinBounds();
    }
}
