namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ScheduledWarrantyManifestEngineChecks
{
    public static void ScheduledWarrantyManifestEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledWarrantyManifestEngine.DefaultScheduledWarrantyManifestEngine();
        var item = ScheduledWarrantyManifest.NewScheduledWarrantyManifest("code-999", "label-483");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledWarrantyManifestEngineKeepsResultsWithinBounds();
    }
}
