namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class ManualWarrantyReservationEngineChecks
{
    public static void ManualWarrantyReservationEngineKeepsResultsWithinBounds()
    {
        var engine = ManualWarrantyReservationEngine.DefaultManualWarrantyReservationEngine(Flags.NewRegistry());
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("code-276", "label-834");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualWarrantyReservationEngineKeepsResultsWithinBounds();
    }
}
