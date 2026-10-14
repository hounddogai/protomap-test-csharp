namespace Acme.Workforce.Assets.Tests.Warranty.Assignment;

using Acme.Workforce.Assets.Warranty.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WarrantyProfileEngineChecks
{
    public static void WarrantyProfileEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyProfileEngine.DefaultWarrantyProfileEngine();
        var item = WarrantyProfile.NewWarrantyProfile("code-139", "title-247");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyProfileEngineKeepsResultsWithinBounds();
    }
}
