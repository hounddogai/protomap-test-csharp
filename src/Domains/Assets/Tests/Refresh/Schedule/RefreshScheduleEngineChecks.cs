namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RefreshScheduleEngineChecks
{
    public static void RefreshScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshScheduleEngine.DefaultRefreshScheduleEngine(Flags.NewRegistry());
        var item = RefreshSchedule.NewRefreshSchedule("code-942", "title-275");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshScheduleEngineKeepsResultsWithinBounds();
    }
}
