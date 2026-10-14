namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedLicenseSeatBundleEngineChecks
{
    public static void WeightedLicenseSeatBundleEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedLicenseSeatBundleEngine.DefaultWeightedLicenseSeatBundleEngine();
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("code-344", "label-691");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatBundleEngineKeepsResultsWithinBounds();
    }
}
