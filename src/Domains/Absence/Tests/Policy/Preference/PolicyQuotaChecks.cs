namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyQuotaChecks
{
    public static void PolicyQuotaStartsInProposed()
    {
        var item = PolicyQuota.NewPolicyQuota("code-453", "name-854");
        Check.Equal(item.Status, PolicyQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyQuotaMovesToApproved()
    {
        var item = PolicyQuota.NewPolicyQuota("code-453", "name-854");
        try
        {
            item.MoveTo(PolicyQuotaStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyQuotaStatus.Approved, "status");
    }

    public static void PolicyQuotaRejectsMovesFromWithdrawn()
    {
        var item = PolicyQuota.NewPolicyQuota("code-453", "name-854");
        item.Status = PolicyQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyQuotaStartsInProposed();
        PolicyQuotaMovesToApproved();
        PolicyQuotaRejectsMovesFromWithdrawn();
    }
}
