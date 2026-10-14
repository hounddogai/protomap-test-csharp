namespace Acme.Workforce.Contractors.Tests.Offboarding.Group;

using Acme.Workforce.Contractors.Offboarding.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class OffboardingClaimEngineChecks
{
    public static void OffboardingClaimEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingClaimEngine.DefaultOffboardingClaimEngine(Flags.NewRegistry());
        var item = OffboardingClaim.NewOffboardingClaim("code-556", "name-788");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingClaimEngineKeepsResultsWithinBounds();
    }
}
