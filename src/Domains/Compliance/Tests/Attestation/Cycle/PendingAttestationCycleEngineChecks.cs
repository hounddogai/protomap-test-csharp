namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PendingAttestationCycleEngineChecks
{
    public static void PendingAttestationCycleEngineKeepsResultsWithinBounds()
    {
        var engine = PendingAttestationCycleEngine.DefaultPendingAttestationCycleEngine(Flags.NewRegistry());
        var item = PendingAttestationCycle.NewPendingAttestationCycle("code-664", "name-938");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingAttestationCycleEngineKeepsResultsWithinBounds();
    }
}
