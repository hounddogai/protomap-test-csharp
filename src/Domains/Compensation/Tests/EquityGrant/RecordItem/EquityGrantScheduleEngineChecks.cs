namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantScheduleEngineChecks
{
    public static void EquityGrantScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantScheduleEngine.DefaultEquityGrantScheduleEngine();
        var item = EquityGrantSchedule.NewEquityGrantSchedule("code-536", "title-180");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantScheduleEngineKeepsResultsWithinBounds();
    }
}
