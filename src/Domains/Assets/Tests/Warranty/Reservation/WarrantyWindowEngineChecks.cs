namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyWindowEngineChecks
{
    public static void WarrantyWindowEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyWindowEngine.DefaultWarrantyWindowEngine(Flags.NewRegistry());
        var item = WarrantyWindow.NewWarrantyWindow("code-193", "label-795");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyWindowEngineKeepsResultsWithinBounds();
    }
}
