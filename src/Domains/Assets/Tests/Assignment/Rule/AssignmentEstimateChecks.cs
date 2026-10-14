namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentEstimateChecks
{
    public static void AssignmentEstimateStartsInPlanned()
    {
        var item = AssignmentEstimate.NewAssignmentEstimate("code-809", "name-62");
        Check.Equal(item.Status, AssignmentEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentEstimateMovesToScheduled()
    {
        var item = AssignmentEstimate.NewAssignmentEstimate("code-809", "name-62");
        try
        {
            item.MoveTo(AssignmentEstimateStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentEstimateStatus.Scheduled, "status");
    }

    public static void AssignmentEstimateRejectsMovesFromCancelled()
    {
        var item = AssignmentEstimate.NewAssignmentEstimate("code-809", "name-62");
        item.Status = AssignmentEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AssignmentEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AssignmentEstimateStartsInPlanned();
        AssignmentEstimateMovesToScheduled();
        AssignmentEstimateRejectsMovesFromCancelled();
    }
}
