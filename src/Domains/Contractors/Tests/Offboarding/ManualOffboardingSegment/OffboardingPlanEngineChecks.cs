namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class OffboardingPlanEngineChecks
{
    public static void OffboardingPlanEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingPlanEngine.DefaultOffboardingPlanEngine();
        var item = OffboardingPlan.NewOffboardingPlan("code-69", "label-649");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingPlanEngineKeepsResultsWithinBounds();
    }
}
