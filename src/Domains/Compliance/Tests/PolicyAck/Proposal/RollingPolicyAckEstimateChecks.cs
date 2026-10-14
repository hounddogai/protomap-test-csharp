namespace Acme.Workforce.Compliance.Tests.PolicyAck.Proposal;

using Acme.Workforce.Compliance.PolicyAck.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RollingPolicyAckEstimateChecks
{
    public static void RollingPolicyAckEstimateStartsInOpen()
    {
        var item = RollingPolicyAckEstimate.NewRollingPolicyAckEstimate("code-286", "title-292");
        Check.Equal(item.Status, RollingPolicyAckEstimateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPolicyAckEstimateMovesToInReview()
    {
        var item = RollingPolicyAckEstimate.NewRollingPolicyAckEstimate("code-286", "title-292");
        try
        {
            item.MoveTo(RollingPolicyAckEstimateStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPolicyAckEstimateStatus.InReview, "status");
    }

    public static void RollingPolicyAckEstimateRejectsMovesFromArchived()
    {
        var item = RollingPolicyAckEstimate.NewRollingPolicyAckEstimate("code-286", "title-292");
        item.Status = RollingPolicyAckEstimateStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingPolicyAckEstimateStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingPolicyAckEstimateStartsInOpen();
        RollingPolicyAckEstimateMovesToInReview();
        RollingPolicyAckEstimateRejectsMovesFromArchived();
    }
}
