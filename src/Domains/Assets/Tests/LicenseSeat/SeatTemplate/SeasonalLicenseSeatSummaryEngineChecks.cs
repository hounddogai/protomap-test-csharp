namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatSummaryEngineChecks
{
    public static void SeasonalLicenseSeatSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalLicenseSeatSummaryEngine.DefaultSeasonalLicenseSeatSummaryEngine();
        var item = SeasonalLicenseSeatSummary.NewSeasonalLicenseSeatSummary("code-649", "title-932");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatSummaryEngineKeepsResultsWithinBounds();
    }
}
