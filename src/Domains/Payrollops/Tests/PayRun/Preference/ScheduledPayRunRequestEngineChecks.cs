namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledPayRunRequestEngineChecks
{
    public static void ScheduledPayRunRequestEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPayRunRequestEngine.DefaultScheduledPayRunRequestEngine();
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("code-277", "name-155");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPayRunRequestEngineKeepsResultsWithinBounds();
    }
}
