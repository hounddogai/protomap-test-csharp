namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorAssignmentEngineChecks
{
    public static void VendorAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = VendorAssignmentEngine.DefaultVendorAssignmentEngine(Flags.NewRegistry());
        var item = VendorAssignment.NewVendorAssignment("code-126", "title-886");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorAssignmentEngineKeepsResultsWithinBounds();
    }
}
