namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class PolicyMilestoneChecks
{
    public static void PolicyMilestoneStartsInRequested()
    {
        var item = PolicyMilestone.NewPolicyMilestone("code-753", "title-971");
        Check.Equal(item.Status, PolicyMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyMilestoneMovesToExpired()
    {
        var item = PolicyMilestone.NewPolicyMilestone("code-753", "title-971");
        try
        {
            item.MoveTo(PolicyMilestoneStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyMilestoneStatus.Expired, "status");
    }

    public static void PolicyMilestoneRejectsMovesFromExpired()
    {
        var item = PolicyMilestone.NewPolicyMilestone("code-753", "title-971");
        item.Status = PolicyMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(PolicyMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PolicyMilestoneStartsInRequested();
        PolicyMilestoneMovesToExpired();
        PolicyMilestoneRejectsMovesFromExpired();
    }
}
