namespace Acme.Workforce.Absence.Tests.Policy.Estimate;

using Acme.Workforce.Absence.Policy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PolicyEstimateChecks
{
    public static void PolicyEstimateStartsInProposed()
    {
        var item = PolicyEstimate.NewPolicyEstimate("code-747", "title-911");
        Check.Equal(item.Status, PolicyEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyEstimateMovesToApproved()
    {
        var item = PolicyEstimate.NewPolicyEstimate("code-747", "title-911");
        try
        {
            item.MoveTo(PolicyEstimateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyEstimateStatus.Approved, "status");
    }

    public static void PolicyEstimateRejectsMovesFromWithdrawn()
    {
        var item = PolicyEstimate.NewPolicyEstimate("code-747", "title-911");
        item.Status = PolicyEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyEstimateStartsInProposed();
        PolicyEstimateMovesToApproved();
        PolicyEstimateRejectsMovesFromWithdrawn();
    }
}
