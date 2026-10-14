namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AttestationPlanEngineChecks
{
    public static void AttestationPlanEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationPlanEngine.DefaultAttestationPlanEngine();
        var item = AttestationPlan.NewAttestationPlan("code-759", "title-681");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationPlanEngineKeepsResultsWithinBounds();
    }
}
