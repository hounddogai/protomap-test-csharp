namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class PolicyAckVarianceChecks
{
    public static void PolicyAckVarianceStartsInCreated()
    {
        var item = PolicyAckVariance.NewPolicyAckVariance("code-941", "title-2");
        Check.Equal(item.Status, PolicyAckVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckVarianceMovesToPublished()
    {
        var item = PolicyAckVariance.NewPolicyAckVariance("code-941", "title-2");
        try
        {
            item.MoveTo(PolicyAckVarianceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckVarianceStatus.Published, "status");
    }

    public static void PolicyAckVarianceRejectsMovesFromRetired()
    {
        var item = PolicyAckVariance.NewPolicyAckVariance("code-941", "title-2");
        item.Status = PolicyAckVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicyAckVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicyAckVarianceStartsInCreated();
        PolicyAckVarianceMovesToPublished();
        PolicyAckVarianceRejectsMovesFromRetired();
    }
}
