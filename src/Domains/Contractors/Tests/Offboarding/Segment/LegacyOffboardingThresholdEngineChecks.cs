namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LegacyOffboardingThresholdEngineChecks
{
    public static void LegacyOffboardingThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyOffboardingThresholdEngine.DefaultLegacyOffboardingThresholdEngine();
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("code-539", "title-406");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyOffboardingThresholdEngineKeepsResultsWithinBounds();
    }
}
