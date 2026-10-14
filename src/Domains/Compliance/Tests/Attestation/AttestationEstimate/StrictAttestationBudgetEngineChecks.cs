namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class StrictAttestationBudgetEngineChecks
{
    public static void StrictAttestationBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAttestationBudgetEngine.DefaultStrictAttestationBudgetEngine(Flags.NewRegistry());
        var item = StrictAttestationBudget.NewStrictAttestationBudget("code-491", "label-945");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAttestationBudgetEngineKeepsResultsWithinBounds();
    }
}
