namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ManualCertificationClaimEngineChecks
{
    public static void ManualCertificationClaimEngineKeepsResultsWithinBounds()
    {
        var engine = ManualCertificationClaimEngine.DefaultManualCertificationClaimEngine(Flags.NewRegistry());
        var item = ManualCertificationClaim.NewManualCertificationClaim("code-283", "name-54");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualCertificationClaimEngineKeepsResultsWithinBounds();
    }
}
