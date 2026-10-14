namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyAssignmentChecks
{
    public static void PolicyAssignmentStartsInDraft()
    {
        var item = PolicyAssignment.NewPolicyAssignment("code-258", "title-612");
        Check.Equal(item.Status, PolicyAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAssignmentMovesToPending()
    {
        var item = PolicyAssignment.NewPolicyAssignment("code-258", "title-612");
        try
        {
            item.MoveTo(PolicyAssignmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAssignmentStatus.Pending, "status");
    }

    public static void PolicyAssignmentRejectsMovesFromClosed()
    {
        var item = PolicyAssignment.NewPolicyAssignment("code-258", "title-612");
        item.Status = PolicyAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(PolicyAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PolicyAssignmentStartsInDraft();
        PolicyAssignmentMovesToPending();
        PolicyAssignmentRejectsMovesFromClosed();
    }
}
