namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationBundleEngineChecks
{
    public static void AttestationBundleEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationBundleEngine.DefaultAttestationBundleEngine(Flags.NewRegistry());
        var item = AttestationBundle.NewAttestationBundle("code-816", "name-823");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationBundleEngineKeepsResultsWithinBounds();
    }
}
