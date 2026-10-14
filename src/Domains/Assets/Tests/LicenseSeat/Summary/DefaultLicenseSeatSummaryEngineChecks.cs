namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultLicenseSeatSummaryEngineChecks
{
    public static void DefaultLicenseSeatSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultLicenseSeatSummaryEngine.DefaultDefaultLicenseSeatSummaryEngine();
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("code-50", "title-305");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatSummaryEngineKeepsResultsWithinBounds();
    }
}
