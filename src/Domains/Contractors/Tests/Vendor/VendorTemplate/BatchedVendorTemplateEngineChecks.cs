namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class BatchedVendorTemplateEngineChecks
{
    public static void BatchedVendorTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedVendorTemplateEngine.DefaultBatchedVendorTemplateEngine(Flags.NewRegistry());
        var item = BatchedVendorTemplate.NewBatchedVendorTemplate("code-423", "title-443");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedVendorTemplateEngineKeepsResultsWithinBounds();
    }
}
