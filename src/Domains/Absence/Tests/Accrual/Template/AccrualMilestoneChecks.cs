namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AccrualMilestoneChecks
{
    public static void AccrualMilestoneStartsInRequested()
    {
        var item = AccrualMilestone.NewAccrualMilestone("code-260", "name-120");
        Check.Equal(item.Status, AccrualMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualMilestoneMovesToRevoked()
    {
        var item = AccrualMilestone.NewAccrualMilestone("code-260", "name-120");
        try
        {
            item.MoveTo(AccrualMilestoneStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualMilestoneStatus.Revoked, "status");
    }

    public static void AccrualMilestoneRejectsMovesFromExpired()
    {
        var item = AccrualMilestone.NewAccrualMilestone("code-260", "name-120");
        item.Status = AccrualMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(AccrualMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccrualMilestoneStartsInRequested();
        AccrualMilestoneMovesToRevoked();
        AccrualMilestoneRejectsMovesFromExpired();
    }
}
