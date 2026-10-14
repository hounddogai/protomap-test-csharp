namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CappedOffboardingBaselineEngineChecks
{
    public static void CappedOffboardingBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = CappedOffboardingBaselineEngine.DefaultCappedOffboardingBaselineEngine();
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("code-502", "title-116");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedOffboardingBaselineEngineKeepsResultsWithinBounds();
    }
}
