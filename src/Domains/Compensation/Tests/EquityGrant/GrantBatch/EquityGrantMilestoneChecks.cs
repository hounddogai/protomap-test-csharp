namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class EquityGrantMilestoneChecks
{
    public static void EquityGrantMilestoneStartsInRequested()
    {
        var item = EquityGrantMilestone.NewEquityGrantMilestone("code-553", "title-740");
        Check.Equal(item.Status, EquityGrantMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantMilestoneMovesToRevoked()
    {
        var item = EquityGrantMilestone.NewEquityGrantMilestone("code-553", "title-740");
        try
        {
            item.MoveTo(EquityGrantMilestoneStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantMilestoneStatus.Revoked, "status");
    }

    public static void EquityGrantMilestoneRejectsMovesFromExpired()
    {
        var item = EquityGrantMilestone.NewEquityGrantMilestone("code-553", "title-740");
        item.Status = EquityGrantMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(EquityGrantMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        EquityGrantMilestoneStartsInRequested();
        EquityGrantMilestoneMovesToRevoked();
        EquityGrantMilestoneRejectsMovesFromExpired();
    }
}
