namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationRuleEngineChecks
{
    public static void AttestationRuleEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationRuleEngine.DefaultAttestationRuleEngine(Flags.NewRegistry());
        var item = AttestationRule.NewAttestationRule("code-179", "title-420");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationRuleEngineKeepsResultsWithinBounds();
    }
}
