namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class FallbackPolicyAckProfileChecks
{
    public static void FallbackPolicyAckProfileStartsInProposed()
    {
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("code-388", "name-346");
        Check.Equal(item.Status, FallbackPolicyAckProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPolicyAckProfileMovesToApproved()
    {
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("code-388", "name-346");
        try
        {
            item.MoveTo(FallbackPolicyAckProfileStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPolicyAckProfileStatus.Approved, "status");
    }

    public static void FallbackPolicyAckProfileRejectsMovesFromWithdrawn()
    {
        var item = FallbackPolicyAckProfile.NewFallbackPolicyAckProfile("code-388", "name-346");
        item.Status = FallbackPolicyAckProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackPolicyAckProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackPolicyAckProfileStartsInProposed();
        FallbackPolicyAckProfileMovesToApproved();
        FallbackPolicyAckProfileRejectsMovesFromWithdrawn();
    }
}
