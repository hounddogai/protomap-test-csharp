namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantyBundleEngineChecks
{
    public static void WarrantyBundleEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyBundleEngine.DefaultWarrantyBundleEngine(Flags.NewRegistry());
        var item = WarrantyBundle.NewWarrantyBundle("code-798", "title-380");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyBundleEngineKeepsResultsWithinBounds();
    }
}
