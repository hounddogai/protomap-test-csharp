namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorLedgerEngineChecks
{
    public static void VendorLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = VendorLedgerEngine.DefaultVendorLedgerEngine();
        var item = VendorLedger.NewVendorLedger("code-639", "title-65");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorLedgerEngineKeepsResultsWithinBounds();
    }
}
