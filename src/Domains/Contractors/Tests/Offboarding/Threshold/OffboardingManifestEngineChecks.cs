namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingManifestEngineChecks
{
    public static void OffboardingManifestEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingManifestEngine.DefaultOffboardingManifestEngine();
        var item = OffboardingManifest.NewOffboardingManifest("code-880", "title-220");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingManifestEngineKeepsResultsWithinBounds();
    }
}
