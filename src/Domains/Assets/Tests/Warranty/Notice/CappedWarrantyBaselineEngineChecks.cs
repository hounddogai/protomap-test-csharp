namespace Acme.Workforce.Assets.Tests.Warranty.Notice;

using Acme.Workforce.Assets.Warranty.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedWarrantyBaselineEngineChecks
{
    public static void CappedWarrantyBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = CappedWarrantyBaselineEngine.DefaultCappedWarrantyBaselineEngine();
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("code-643", "label-816");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedWarrantyBaselineEngineKeepsResultsWithinBounds();
    }
}
