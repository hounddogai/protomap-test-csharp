namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientVendorReservationEngineChecks
{
    public static void LenientVendorReservationEngineKeepsResultsWithinBounds()
    {
        var engine = LenientVendorReservationEngine.DefaultLenientVendorReservationEngine();
        var item = LenientVendorReservation.NewLenientVendorReservation("code-153", "name-378");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientVendorReservationEngineKeepsResultsWithinBounds();
    }
}
