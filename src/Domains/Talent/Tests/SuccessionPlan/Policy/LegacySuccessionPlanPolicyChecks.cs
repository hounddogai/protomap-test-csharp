namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Policy;

using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LegacySuccessionPlanPolicyChecks
{
    public static void LegacySuccessionPlanPolicyStartsInProposed()
    {
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("code-73", "label-872");
        Check.Equal(item.Status, LegacySuccessionPlanPolicyStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacySuccessionPlanPolicyMovesToApproved()
    {
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("code-73", "label-872");
        try
        {
            item.MoveTo(LegacySuccessionPlanPolicyStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacySuccessionPlanPolicyStatus.Approved, "status");
    }

    public static void LegacySuccessionPlanPolicyRejectsMovesFromWithdrawn()
    {
        var item = LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy("code-73", "label-872");
        item.Status = LegacySuccessionPlanPolicyStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacySuccessionPlanPolicyStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacySuccessionPlanPolicyStartsInProposed();
        LegacySuccessionPlanPolicyMovesToApproved();
        LegacySuccessionPlanPolicyRejectsMovesFromWithdrawn();
    }
}
