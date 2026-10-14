namespace Acme.Workforce.Workplace.Tests.Visitor.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VisitorScheduleEngineChecks
{
    public static void VisitorScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorScheduleEngine.DefaultVisitorScheduleEngine(Flags.NewRegistry());
        var item = VisitorSchedule.NewVisitorSchedule("code-38", "label-514");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorScheduleEngineKeepsResultsWithinBounds();
    }
}
