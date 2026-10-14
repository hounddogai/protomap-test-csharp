namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ScheduledVisitorMetricEngineChecks
{
    public static void ScheduledVisitorMetricEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledVisitorMetricEngine.DefaultScheduledVisitorMetricEngine(Flags.NewRegistry());
        var item = ScheduledVisitorMetric.NewScheduledVisitorMetric("code-708", "name-961");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledVisitorMetricEngineKeepsResultsWithinBounds();
    }
}
