namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBatchEngineChecks
{
    public static void LenientVendorBatchEngineKeepsResultsWithinBounds()
    {
        var engine = LenientVendorBatchEngine.DefaultLenientVendorBatchEngine();
        var item = LenientVendorBatch.NewLenientVendorBatch("code-431", "label-41");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientVendorBatchEngineKeepsResultsWithinBounds();
    }
}
