namespace Acme.Workforce.Compliance.Tests.PolicyAck.Bundle;

using Acme.Workforce.Compliance.PolicyAck.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PolicyAckEstimateChecks
{
    public static void PolicyAckEstimateStartsInDraft()
    {
        var item = PolicyAckEstimate.NewPolicyAckEstimate("code-653", "name-366");
        Check.Equal(item.Status, PolicyAckEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckEstimateMovesToPending()
    {
        var item = PolicyAckEstimate.NewPolicyAckEstimate("code-653", "name-366");
        try
        {
            item.MoveTo(PolicyAckEstimateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckEstimateStatus.Pending, "status");
    }

    public static void PolicyAckEstimateRejectsMovesFromClosed()
    {
        var item = PolicyAckEstimate.NewPolicyAckEstimate("code-653", "name-366");
        item.Status = PolicyAckEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(PolicyAckEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PolicyAckEstimateStartsInDraft();
        PolicyAckEstimateMovesToPending();
        PolicyAckEstimateRejectsMovesFromClosed();
    }
}
