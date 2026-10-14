namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class WarrantySlotEngineChecks
{
    public static void WarrantySlotEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantySlotEngine.DefaultWarrantySlotEngine(Flags.NewRegistry());
        var item = WarrantySlot.NewWarrantySlot("code-33", "label-806");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantySlotEngineKeepsResultsWithinBounds();
    }
}
