namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedPolicyAckBundleChecks
{
    public static void WeightedPolicyAckBundleStartsInProposed()
    {
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("code-305", "label-926");
        Check.Equal(item.Status, WeightedPolicyAckBundleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedPolicyAckBundleMovesToApproved()
    {
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("code-305", "label-926");
        try
        {
            item.MoveTo(WeightedPolicyAckBundleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedPolicyAckBundleStatus.Approved, "status");
    }

    public static void WeightedPolicyAckBundleRejectsMovesFromWithdrawn()
    {
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("code-305", "label-926");
        item.Status = WeightedPolicyAckBundleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedPolicyAckBundleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedPolicyAckBundleStartsInProposed();
        WeightedPolicyAckBundleMovesToApproved();
        WeightedPolicyAckBundleRejectsMovesFromWithdrawn();
    }
}
