namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyWarrantyBaselineEngineChecks
{
    public static void LegacyWarrantyBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyWarrantyBaselineEngine.DefaultLegacyWarrantyBaselineEngine();
        var item = LegacyWarrantyBaseline.NewLegacyWarrantyBaseline("code-627", "label-164");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyWarrantyBaselineEngineKeepsResultsWithinBounds();
    }
}
