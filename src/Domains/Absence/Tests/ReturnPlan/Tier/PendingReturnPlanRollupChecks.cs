namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PendingReturnPlanRollupChecks
{
    public static void PendingReturnPlanRollupStartsInCreated()
    {
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("code-562", "title-355");
        Check.Equal(item.Status, PendingReturnPlanRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingReturnPlanRollupMovesToRetired()
    {
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("code-562", "title-355");
        try
        {
            item.MoveTo(PendingReturnPlanRollupStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingReturnPlanRollupStatus.Retired, "status");
    }

    public static void PendingReturnPlanRollupRejectsMovesFromRetired()
    {
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("code-562", "title-355");
        item.Status = PendingReturnPlanRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingReturnPlanRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingReturnPlanRollupStartsInCreated();
        PendingReturnPlanRollupMovesToRetired();
        PendingReturnPlanRollupRejectsMovesFromRetired();
    }
}
