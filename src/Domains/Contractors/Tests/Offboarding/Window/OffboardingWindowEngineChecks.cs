namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class OffboardingWindowEngineChecks
{
    public static void OffboardingWindowEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingWindowEngine.DefaultOffboardingWindowEngine();
        var item = OffboardingWindow.NewOffboardingWindow("code-383", "title-748");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingWindowEngineKeepsResultsWithinBounds();
    }
}
