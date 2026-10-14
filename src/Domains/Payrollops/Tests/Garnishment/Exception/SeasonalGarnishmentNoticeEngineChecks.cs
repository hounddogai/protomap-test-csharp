namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SeasonalGarnishmentNoticeEngineChecks
{
    public static void SeasonalGarnishmentNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalGarnishmentNoticeEngine.DefaultSeasonalGarnishmentNoticeEngine();
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("code-855", "label-764");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentNoticeEngineKeepsResultsWithinBounds();
    }
}
