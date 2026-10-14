namespace Acme.Workforce.Compensation.Tests.BonusPool.Schedule;

using Acme.Workforce.Compensation.BonusPool.Schedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BonusPoolScheduleEngineChecks
{
    public static void BonusPoolScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolScheduleEngine.DefaultBonusPoolScheduleEngine(Flags.NewRegistry());
        var item = BonusPoolSchedule.NewBonusPoolSchedule("code-576", "title-279");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolScheduleEngineKeepsResultsWithinBounds();
    }
}
