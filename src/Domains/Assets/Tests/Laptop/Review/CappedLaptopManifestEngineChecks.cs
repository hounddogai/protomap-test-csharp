namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedLaptopManifestEngineChecks
{
    public static void CappedLaptopManifestEngineKeepsResultsWithinBounds()
    {
        var engine = CappedLaptopManifestEngine.DefaultCappedLaptopManifestEngine(Flags.NewRegistry());
        var item = CappedLaptopManifest.NewCappedLaptopManifest("code-621", "name-317");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedLaptopManifestEngineKeepsResultsWithinBounds();
    }
}
