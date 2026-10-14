namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationRuleChecks
{
    public static void AttestationRuleStartsInIdle()
    {
        var item = AttestationRule.NewAttestationRule("code-179", "title-420");
        Check.Equal(item.Status, AttestationRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationRuleMovesToStopped()
    {
        var item = AttestationRule.NewAttestationRule("code-179", "title-420");
        try
        {
            item.MoveTo(AttestationRuleStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationRuleStatus.Stopped, "status");
    }

    public static void AttestationRuleRejectsMovesFromStopped()
    {
        var item = AttestationRule.NewAttestationRule("code-179", "title-420");
        item.Status = AttestationRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(AttestationRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AttestationRuleStartsInIdle();
        AttestationRuleMovesToStopped();
        AttestationRuleRejectsMovesFromStopped();
    }
}
