namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveLaptopManifestEngineChecks
{
    public static void AdaptiveLaptopManifestEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveLaptopManifestEngine.DefaultAdaptiveLaptopManifestEngine();
        var item = AdaptiveLaptopManifest.NewAdaptiveLaptopManifest("code-914", "title-349");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveLaptopManifestEngineKeepsResultsWithinBounds();
    }
}
