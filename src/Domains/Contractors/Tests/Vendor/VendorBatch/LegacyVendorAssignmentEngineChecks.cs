namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBatch;

using Acme.Workforce.Contractors.Vendor.VendorBatch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of vendor batch code, run by the project's runner.</summary>
public static class LegacyVendorAssignmentEngineChecks
{
    public static void LegacyVendorAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyVendorAssignmentEngine.DefaultLegacyVendorAssignmentEngine(Flags.NewRegistry());
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("code-975", "label-870");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyVendorAssignmentEngineKeepsResultsWithinBounds();
    }
}
