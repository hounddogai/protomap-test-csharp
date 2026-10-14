namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationMappingEngineChecks
{
    public static void AttestationMappingEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationMappingEngine.DefaultAttestationMappingEngine(Flags.NewRegistry());
        var item = AttestationMapping.NewAttestationMapping("code-425", "label-236");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationMappingEngineKeepsResultsWithinBounds();
    }
}
