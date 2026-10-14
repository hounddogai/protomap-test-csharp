namespace Acme.Workforce.Assets.Tests.Laptop.LaptopManifest;

using Acme.Workforce.Assets.Laptop.LaptopManifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of laptop manifest code, run by the project's runner.</summary>
public static class ManualLaptopBundleEngineChecks
{
    public static void ManualLaptopBundleEngineKeepsResultsWithinBounds()
    {
        var engine = ManualLaptopBundleEngine.DefaultManualLaptopBundleEngine(Flags.NewRegistry());
        var item = ManualLaptopBundle.NewManualLaptopBundle("code-423", "name-939");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualLaptopBundleEngineKeepsResultsWithinBounds();
    }
}
