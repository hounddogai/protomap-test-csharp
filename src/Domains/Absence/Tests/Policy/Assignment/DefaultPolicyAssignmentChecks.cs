namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultPolicyAssignmentChecks
{
    public static void DefaultPolicyAssignmentStartsInDraft()
    {
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("code-296", "name-825");
        Check.Equal(item.Status, DefaultPolicyAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPolicyAssignmentMovesToActive()
    {
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("code-296", "name-825");
        try
        {
            item.MoveTo(DefaultPolicyAssignmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPolicyAssignmentStatus.Active, "status");
    }

    public static void DefaultPolicyAssignmentRejectsMovesFromClosed()
    {
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("code-296", "name-825");
        item.Status = DefaultPolicyAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultPolicyAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultPolicyAssignmentStartsInDraft();
        DefaultPolicyAssignmentMovesToActive();
        DefaultPolicyAssignmentRejectsMovesFromClosed();
    }
}
