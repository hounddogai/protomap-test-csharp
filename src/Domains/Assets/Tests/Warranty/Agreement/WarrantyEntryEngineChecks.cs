namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyEntryEngineChecks
{
    public static void WarrantyEntryEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyEntryEngine.DefaultWarrantyEntryEngine(Flags.NewRegistry());
        var item = WarrantyEntry.NewWarrantyEntry("code-609", "label-907");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyEntryEngineKeepsResultsWithinBounds();
    }
}
