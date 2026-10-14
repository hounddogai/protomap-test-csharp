namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class PrimaryVendorProfileEngineChecks
{
    public static void PrimaryVendorProfileEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryVendorProfileEngine.DefaultPrimaryVendorProfileEngine();
        var item = PrimaryVendorProfile.NewPrimaryVendorProfile("code-259", "title-602");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryVendorProfileEngineKeepsResultsWithinBounds();
    }
}
