namespace Acme.Workforce.Compliance.Tests.PolicyAck.Estimate;

using Acme.Workforce.Compliance.PolicyAck.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryPolicyAckPlanChecks
{
    public static void PrimaryPolicyAckPlanStartsInDraft()
    {
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("code-392", "label-505");
        Check.Equal(item.Status, PrimaryPolicyAckPlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPolicyAckPlanMovesToSuspended()
    {
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("code-392", "label-505");
        try
        {
            item.MoveTo(PrimaryPolicyAckPlanStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPolicyAckPlanStatus.Suspended, "status");
    }

    public static void PrimaryPolicyAckPlanRejectsMovesFromClosed()
    {
        var item = PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan("code-392", "label-505");
        item.Status = PrimaryPolicyAckPlanStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryPolicyAckPlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckPlanStartsInDraft();
        PrimaryPolicyAckPlanMovesToSuspended();
        PrimaryPolicyAckPlanRejectsMovesFromClosed();
    }
}
