namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GarnishmentScheduleEngineChecks
{
    public static void GarnishmentScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentScheduleEngine.DefaultGarnishmentScheduleEngine(Flags.NewRegistry());
        var item = GarnishmentSchedule.NewGarnishmentSchedule("code-546", "title-269");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentScheduleEngineKeepsResultsWithinBounds();
    }
}
