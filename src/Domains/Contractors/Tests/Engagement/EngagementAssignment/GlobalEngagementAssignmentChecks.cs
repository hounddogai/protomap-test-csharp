namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class GlobalEngagementAssignmentChecks
{
    public static void GlobalEngagementAssignmentStartsInNew()
    {
        var item = GlobalEngagementAssignment.NewGlobalEngagementAssignment("code-395", "title-367");
        Check.Equal(item.Status, GlobalEngagementAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalEngagementAssignmentMovesToDone()
    {
        var item = GlobalEngagementAssignment.NewGlobalEngagementAssignment("code-395", "title-367");
        try
        {
            item.MoveTo(GlobalEngagementAssignmentStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalEngagementAssignmentStatus.Done, "status");
    }

    public static void GlobalEngagementAssignmentRejectsMovesFromFailed()
    {
        var item = GlobalEngagementAssignment.NewGlobalEngagementAssignment("code-395", "title-367");
        item.Status = GlobalEngagementAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalEngagementAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalEngagementAssignmentStartsInNew();
        GlobalEngagementAssignmentMovesToDone();
        GlobalEngagementAssignmentRejectsMovesFromFailed();
    }
}
