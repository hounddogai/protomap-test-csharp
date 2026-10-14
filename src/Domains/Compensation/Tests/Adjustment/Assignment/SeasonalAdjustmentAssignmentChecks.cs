namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class SeasonalAdjustmentAssignmentChecks
{
    public static void SeasonalAdjustmentAssignmentStartsInOpen()
    {
        var item = SeasonalAdjustmentAssignment.NewSeasonalAdjustmentAssignment("code-66", "title-876");
        Check.Equal(item.Status, SeasonalAdjustmentAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAdjustmentAssignmentMovesToResolved()
    {
        var item = SeasonalAdjustmentAssignment.NewSeasonalAdjustmentAssignment("code-66", "title-876");
        try
        {
            item.MoveTo(SeasonalAdjustmentAssignmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAdjustmentAssignmentStatus.Resolved, "status");
    }

    public static void SeasonalAdjustmentAssignmentRejectsMovesFromArchived()
    {
        var item = SeasonalAdjustmentAssignment.NewSeasonalAdjustmentAssignment("code-66", "title-876");
        item.Status = SeasonalAdjustmentAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(SeasonalAdjustmentAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentAssignmentStartsInOpen();
        SeasonalAdjustmentAssignmentMovesToResolved();
        SeasonalAdjustmentAssignmentRejectsMovesFromArchived();
    }
}
