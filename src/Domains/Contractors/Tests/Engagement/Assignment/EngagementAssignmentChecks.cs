namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementAssignmentChecks
{
    public static void EngagementAssignmentStartsInPlanned()
    {
        var item = EngagementAssignment.NewEngagementAssignment("code-578", "title-992");
        Check.Equal(item.Status, EngagementAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementAssignmentMovesToScheduled()
    {
        var item = EngagementAssignment.NewEngagementAssignment("code-578", "title-992");
        try
        {
            item.MoveTo(EngagementAssignmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementAssignmentStatus.Scheduled, "status");
    }

    public static void EngagementAssignmentRejectsMovesFromCancelled()
    {
        var item = EngagementAssignment.NewEngagementAssignment("code-578", "title-992");
        item.Status = EngagementAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EngagementAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EngagementAssignmentStartsInPlanned();
        EngagementAssignmentMovesToScheduled();
        EngagementAssignmentRejectsMovesFromCancelled();
    }
}
