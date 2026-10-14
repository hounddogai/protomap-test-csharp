namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientVendorBaselineEngineChecks
{
    public static void LenientVendorBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = LenientVendorBaselineEngine.DefaultLenientVendorBaselineEngine(Flags.NewRegistry());
        var item = LenientVendorBaseline.NewLenientVendorBaseline("code-225", "title-22");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientVendorBaselineEngineKeepsResultsWithinBounds();
    }
}
