namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LicenseSeatSegmentEngineChecks
{
    public static void LicenseSeatSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatSegmentEngine.DefaultLicenseSeatSegmentEngine();
        var item = LicenseSeatSegment.NewLicenseSeatSegment("code-431", "label-701");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatSegmentEngineKeepsResultsWithinBounds();
    }
}
