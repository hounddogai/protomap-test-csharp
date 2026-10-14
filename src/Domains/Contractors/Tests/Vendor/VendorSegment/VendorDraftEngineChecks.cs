namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorDraftEngineChecks
{
    public static void VendorDraftEngineKeepsResultsWithinBounds()
    {
        var engine = VendorDraftEngine.DefaultVendorDraftEngine();
        var item = VendorDraft.NewVendorDraft("code-548", "title-748");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorDraftEngineKeepsResultsWithinBounds();
    }
}
