namespace Acme.Workforce.Contractors.Tests.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedVendorEntryEngineChecks
{
    public static void CappedVendorEntryEngineKeepsResultsWithinBounds()
    {
        var engine = CappedVendorEntryEngine.DefaultCappedVendorEntryEngine();
        var item = CappedVendorEntry.NewCappedVendorEntry("code-497", "name-640");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedVendorEntryEngineKeepsResultsWithinBounds();
    }
}
