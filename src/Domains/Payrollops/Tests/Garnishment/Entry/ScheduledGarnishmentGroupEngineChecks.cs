namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ScheduledGarnishmentGroupEngineChecks
{
    public static void ScheduledGarnishmentGroupEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledGarnishmentGroupEngine.DefaultScheduledGarnishmentGroupEngine();
        var item = ScheduledGarnishmentGroup.NewScheduledGarnishmentGroup("code-236", "name-743");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledGarnishmentGroupEngineKeepsResultsWithinBounds();
    }
}
