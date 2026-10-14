namespace Acme.Workforce.Assets.Tests.Assignment.Estimate;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScheduledAssignmentEstimateChecks
{
    public static void ScheduledAssignmentEstimateStartsInDraft()
    {
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("code-625", "label-906");
        Check.Equal(item.Status, ScheduledAssignmentEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAssignmentEstimateMovesToActive()
    {
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("code-625", "label-906");
        try
        {
            item.MoveTo(ScheduledAssignmentEstimateStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAssignmentEstimateStatus.Active, "status");
    }

    public static void ScheduledAssignmentEstimateRejectsMovesFromClosed()
    {
        var item = ScheduledAssignmentEstimate.NewScheduledAssignmentEstimate("code-625", "label-906");
        item.Status = ScheduledAssignmentEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledAssignmentEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledAssignmentEstimateStartsInDraft();
        ScheduledAssignmentEstimateMovesToActive();
        ScheduledAssignmentEstimateRejectsMovesFromClosed();
    }
}
