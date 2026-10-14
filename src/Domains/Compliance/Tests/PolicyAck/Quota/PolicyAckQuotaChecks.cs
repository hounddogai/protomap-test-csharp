namespace Acme.Workforce.Compliance.Tests.PolicyAck.Quota;

using Acme.Workforce.Compliance.PolicyAck.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PolicyAckQuotaChecks
{
    public static void PolicyAckQuotaStartsInCreated()
    {
        var item = PolicyAckQuota.NewPolicyAckQuota("code-663", "title-934");
        Check.Equal(item.Status, PolicyAckQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckQuotaMovesToValidated()
    {
        var item = PolicyAckQuota.NewPolicyAckQuota("code-663", "title-934");
        try
        {
            item.MoveTo(PolicyAckQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckQuotaStatus.Validated, "status");
    }

    public static void PolicyAckQuotaRejectsMovesFromRetired()
    {
        var item = PolicyAckQuota.NewPolicyAckQuota("code-663", "title-934");
        item.Status = PolicyAckQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicyAckQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicyAckQuotaStartsInCreated();
        PolicyAckQuotaMovesToValidated();
        PolicyAckQuotaRejectsMovesFromRetired();
    }
}
