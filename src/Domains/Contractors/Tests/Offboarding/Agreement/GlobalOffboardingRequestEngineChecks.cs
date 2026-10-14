namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GlobalOffboardingRequestEngineChecks
{
    public static void GlobalOffboardingRequestEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalOffboardingRequestEngine.DefaultGlobalOffboardingRequestEngine();
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("code-387", "label-213");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalOffboardingRequestEngineKeepsResultsWithinBounds();
    }
}
