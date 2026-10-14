namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class RegionalPolicyEstimateChecks
{
    public static void RegionalPolicyEstimateStartsInProposed()
    {
        var item = RegionalPolicyEstimate.NewRegionalPolicyEstimate("code-667", "label-753");
        Check.Equal(item.Status, RegionalPolicyEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPolicyEstimateMovesToApproved()
    {
        var item = RegionalPolicyEstimate.NewRegionalPolicyEstimate("code-667", "label-753");
        try
        {
            item.MoveTo(RegionalPolicyEstimateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPolicyEstimateStatus.Approved, "status");
    }

    public static void RegionalPolicyEstimateRejectsMovesFromWithdrawn()
    {
        var item = RegionalPolicyEstimate.NewRegionalPolicyEstimate("code-667", "label-753");
        item.Status = RegionalPolicyEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalPolicyEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalPolicyEstimateStartsInProposed();
        RegionalPolicyEstimateMovesToApproved();
        RegionalPolicyEstimateRejectsMovesFromWithdrawn();
    }
}
