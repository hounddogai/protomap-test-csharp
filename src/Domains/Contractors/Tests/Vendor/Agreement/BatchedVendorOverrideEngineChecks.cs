namespace Acme.Workforce.Contractors.Tests.Vendor.Agreement;

using Acme.Workforce.Contractors.Vendor.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class BatchedVendorOverrideEngineChecks
{
    public static void BatchedVendorOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedVendorOverrideEngine.DefaultBatchedVendorOverrideEngine(Flags.NewRegistry());
        var item = BatchedVendorOverride.NewBatchedVendorOverride("code-60", "name-147");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedVendorOverrideEngineKeepsResultsWithinBounds();
    }
}
