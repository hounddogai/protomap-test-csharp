namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationRule;

using Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>Checks of attestation rule code, run by the project's runner.</summary>
public static class DefaultAttestationRuleChecks
{
    public static void DefaultAttestationRuleStartsInPlanned()
    {
        var item = DefaultAttestationRule.NewDefaultAttestationRule("code-157", "name-78");
        Check.Equal(item.Status, DefaultAttestationRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAttestationRuleMovesToScheduled()
    {
        var item = DefaultAttestationRule.NewDefaultAttestationRule("code-157", "name-78");
        try
        {
            item.MoveTo(DefaultAttestationRuleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAttestationRuleStatus.Scheduled, "status");
    }

    public static void DefaultAttestationRuleRejectsMovesFromCancelled()
    {
        var item = DefaultAttestationRule.NewDefaultAttestationRule("code-157", "name-78");
        item.Status = DefaultAttestationRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultAttestationRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultAttestationRuleStartsInPlanned();
        DefaultAttestationRuleMovesToScheduled();
        DefaultAttestationRuleRejectsMovesFromCancelled();
    }
}
