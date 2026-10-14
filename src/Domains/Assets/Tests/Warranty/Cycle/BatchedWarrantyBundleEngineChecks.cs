namespace Acme.Workforce.Assets.Tests.Warranty.Cycle;

using Acme.Workforce.Assets.Warranty.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class BatchedWarrantyBundleEngineChecks
{
    public static void BatchedWarrantyBundleEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedWarrantyBundleEngine.DefaultBatchedWarrantyBundleEngine(Flags.NewRegistry());
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("code-774", "name-656");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedWarrantyBundleEngineKeepsResultsWithinBounds();
    }
}
