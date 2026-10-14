namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanScheduleEngineChecks
{
    public static void SpanScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = SpanScheduleEngine.DefaultSpanScheduleEngine(Flags.NewRegistry());
        var item = SpanSchedule.NewSpanSchedule("code-160", "name-882");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanScheduleEngineKeepsResultsWithinBounds();
    }
}
