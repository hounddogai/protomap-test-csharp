namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentScheduleChecks
{
    public static void AssignmentScheduleStartsInOpen()
    {
        var item = AssignmentSchedule.NewAssignmentSchedule("code-250", "label-924");
        Check.Equal(item.Status, AssignmentScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentScheduleMovesToReopened()
    {
        var item = AssignmentSchedule.NewAssignmentSchedule("code-250", "label-924");
        try
        {
            item.MoveTo(AssignmentScheduleStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentScheduleStatus.Reopened, "status");
    }

    public static void AssignmentScheduleRejectsMovesFromArchived()
    {
        var item = AssignmentSchedule.NewAssignmentSchedule("code-250", "label-924");
        item.Status = AssignmentScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(AssignmentScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssignmentScheduleStartsInOpen();
        AssignmentScheduleMovesToReopened();
        AssignmentScheduleRejectsMovesFromArchived();
    }
}
