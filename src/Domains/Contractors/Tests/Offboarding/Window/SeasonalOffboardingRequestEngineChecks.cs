namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalOffboardingRequestEngineChecks
{
    public static void SeasonalOffboardingRequestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalOffboardingRequestEngine.DefaultSeasonalOffboardingRequestEngine();
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("code-441", "label-613");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalOffboardingRequestEngineKeepsResultsWithinBounds();
    }
}
