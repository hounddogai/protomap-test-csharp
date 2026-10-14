namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingSegmentEngineChecks
{
    public static void AdaptiveOffboardingSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveOffboardingSegmentEngine.DefaultAdaptiveOffboardingSegmentEngine(Flags.NewRegistry());
        var item = AdaptiveOffboardingSegment.NewAdaptiveOffboardingSegment("code-108", "title-309");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveOffboardingSegmentEngineKeepsResultsWithinBounds();
    }
}
