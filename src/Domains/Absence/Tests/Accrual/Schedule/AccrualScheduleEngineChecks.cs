namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AccrualScheduleEngineChecks
{
    public static void AccrualScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualScheduleEngine.DefaultAccrualScheduleEngine(Flags.NewRegistry());
        var item = AccrualSchedule.NewAccrualSchedule("code-847", "label-38");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualScheduleEngineKeepsResultsWithinBounds();
    }
}
