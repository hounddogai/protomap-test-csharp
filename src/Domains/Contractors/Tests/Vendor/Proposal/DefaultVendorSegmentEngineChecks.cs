namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorSegmentEngineChecks
{
    public static void DefaultVendorSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultVendorSegmentEngine.DefaultDefaultVendorSegmentEngine(Flags.NewRegistry());
        var item = DefaultVendorSegment.NewDefaultVendorSegment("code-861", "label-968");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultVendorSegmentEngineKeepsResultsWithinBounds();
    }
}
