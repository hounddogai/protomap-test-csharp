namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingTierEngineChecks
{
    public static void OffboardingTierEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingTierEngine.DefaultOffboardingTierEngine(Flags.NewRegistry());
        var item = OffboardingTier.NewOffboardingTier("code-527", "label-827");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingTierEngineKeepsResultsWithinBounds();
    }
}
