namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PositionScheduleEngineChecks
{
    public static void PositionScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = PositionScheduleEngine.DefaultPositionScheduleEngine(Flags.NewRegistry());
        var item = PositionSchedule.NewPositionSchedule("code-938", "label-989");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionScheduleEngineKeepsResultsWithinBounds();
    }
}
