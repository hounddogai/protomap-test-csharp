namespace Acme.Workforce.Orgdesign.Tests.Span.Schedule;

using Acme.Workforce.Orgdesign.Span.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualSpanScheduleEngineChecks
{
    public static void ManualSpanScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = ManualSpanScheduleEngine.DefaultManualSpanScheduleEngine();
        var item = ManualSpanSchedule.NewManualSpanSchedule("code-869", "title-276");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualSpanScheduleEngineKeepsResultsWithinBounds();
    }
}
