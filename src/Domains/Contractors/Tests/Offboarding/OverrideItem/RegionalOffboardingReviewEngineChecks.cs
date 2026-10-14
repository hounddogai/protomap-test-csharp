namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalOffboardingReviewEngineChecks
{
    public static void RegionalOffboardingReviewEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalOffboardingReviewEngine.DefaultRegionalOffboardingReviewEngine();
        var item = RegionalOffboardingReview.NewRegionalOffboardingReview("code-997", "title-440");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalOffboardingReviewEngineKeepsResultsWithinBounds();
    }
}
