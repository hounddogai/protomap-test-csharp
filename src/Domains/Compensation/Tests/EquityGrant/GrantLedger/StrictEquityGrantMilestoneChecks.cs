namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class StrictEquityGrantMilestoneChecks
{
    public static void StrictEquityGrantMilestoneStartsInProposed()
    {
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("code-211", "title-786");
        Check.Equal(item.Status, StrictEquityGrantMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictEquityGrantMilestoneMovesToApproved()
    {
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("code-211", "title-786");
        try
        {
            item.MoveTo(StrictEquityGrantMilestoneStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictEquityGrantMilestoneStatus.Approved, "status");
    }

    public static void StrictEquityGrantMilestoneRejectsMovesFromWithdrawn()
    {
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("code-211", "title-786");
        item.Status = StrictEquityGrantMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictEquityGrantMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictEquityGrantMilestoneStartsInProposed();
        StrictEquityGrantMilestoneMovesToApproved();
        StrictEquityGrantMilestoneRejectsMovesFromWithdrawn();
    }
}
