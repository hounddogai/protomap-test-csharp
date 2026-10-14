namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingPolicyEngineChecks
{
    public static void OffboardingPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingPolicyEngine.DefaultOffboardingPolicyEngine(Flags.NewRegistry());
        var item = OffboardingPolicy.NewOffboardingPolicy("code-331", "title-199");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingPolicyEngineKeepsResultsWithinBounds();
    }
}
