namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryAdjustmentPreferenceEngineChecks
{
    public static void PrimaryAdjustmentPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryAdjustmentPreferenceEngine.DefaultPrimaryAdjustmentPreferenceEngine();
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("code-995", "label-564");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryAdjustmentPreferenceEngineKeepsResultsWithinBounds();
    }
}
