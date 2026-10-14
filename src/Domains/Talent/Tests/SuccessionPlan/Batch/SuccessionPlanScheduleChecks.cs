namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanScheduleChecks
{
    public static void SuccessionPlanScheduleStartsInProposed()
    {
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("code-387", "label-967");
        Check.Equal(item.Status, SuccessionPlanScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanScheduleMovesToWithdrawn()
    {
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("code-387", "label-967");
        try
        {
            item.MoveTo(SuccessionPlanScheduleStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanScheduleStatus.Withdrawn, "status");
    }

    public static void SuccessionPlanScheduleRejectsMovesFromWithdrawn()
    {
        var item = SuccessionPlanSchedule.NewSuccessionPlanSchedule("code-387", "label-967");
        item.Status = SuccessionPlanScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SuccessionPlanScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanScheduleStartsInProposed();
        SuccessionPlanScheduleMovesToWithdrawn();
        SuccessionPlanScheduleRejectsMovesFromWithdrawn();
    }
}
