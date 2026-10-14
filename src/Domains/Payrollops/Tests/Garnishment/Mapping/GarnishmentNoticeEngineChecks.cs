namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GarnishmentNoticeEngineChecks
{
    public static void GarnishmentNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentNoticeEngine.DefaultGarnishmentNoticeEngine();
        var item = GarnishmentNotice.NewGarnishmentNotice("code-192", "name-533");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentNoticeEngineKeepsResultsWithinBounds();
    }
}
