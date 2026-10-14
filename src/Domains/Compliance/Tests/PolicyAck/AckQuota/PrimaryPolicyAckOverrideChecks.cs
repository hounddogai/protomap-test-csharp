namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Checks of ack quota code, run by the project's runner.</summary>
public static class PrimaryPolicyAckOverrideChecks
{
    public static void PrimaryPolicyAckOverrideStartsInOpen()
    {
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("code-681", "title-831");
        Check.Equal(item.Status, PrimaryPolicyAckOverrideStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPolicyAckOverrideMovesToResolved()
    {
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("code-681", "title-831");
        try
        {
            item.MoveTo(PrimaryPolicyAckOverrideStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPolicyAckOverrideStatus.Resolved, "status");
    }

    public static void PrimaryPolicyAckOverrideRejectsMovesFromArchived()
    {
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("code-681", "title-831");
        item.Status = PrimaryPolicyAckOverrideStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryPolicyAckOverrideStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckOverrideStartsInOpen();
        PrimaryPolicyAckOverrideMovesToResolved();
        PrimaryPolicyAckOverrideRejectsMovesFromArchived();
    }
}
