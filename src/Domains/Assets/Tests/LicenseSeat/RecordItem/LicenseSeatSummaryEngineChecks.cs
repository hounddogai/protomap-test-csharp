namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LicenseSeatSummaryEngineChecks
{
    public static void LicenseSeatSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatSummaryEngine.DefaultLicenseSeatSummaryEngine();
        var item = LicenseSeatSummary.NewLicenseSeatSummary("code-189", "title-520");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatSummaryEngineKeepsResultsWithinBounds();
    }
}
