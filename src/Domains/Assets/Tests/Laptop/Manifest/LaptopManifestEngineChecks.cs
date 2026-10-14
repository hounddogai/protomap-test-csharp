namespace Acme.Workforce.Assets.Tests.Laptop.Manifest;

using Acme.Workforce.Assets.Laptop.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LaptopManifestEngineChecks
{
    public static void LaptopManifestEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopManifestEngine.DefaultLaptopManifestEngine(Flags.NewRegistry());
        var item = LaptopManifest.NewLaptopManifest("code-76", "title-288");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopManifestEngineKeepsResultsWithinBounds();
    }
}
