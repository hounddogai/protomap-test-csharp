namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class StrictPolicyAssignmentChecks
{
    public static void StrictPolicyAssignmentStartsInDraft()
    {
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("code-465", "label-471");
        Check.Equal(item.Status, StrictPolicyAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPolicyAssignmentMovesToPending()
    {
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("code-465", "label-471");
        try
        {
            item.MoveTo(StrictPolicyAssignmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPolicyAssignmentStatus.Pending, "status");
    }

    public static void StrictPolicyAssignmentRejectsMovesFromClosed()
    {
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("code-465", "label-471");
        item.Status = StrictPolicyAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictPolicyAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictPolicyAssignmentStartsInDraft();
        StrictPolicyAssignmentMovesToPending();
        StrictPolicyAssignmentRejectsMovesFromClosed();
    }
}
