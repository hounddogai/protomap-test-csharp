namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalPolicyAssignmentChecks
{
    public static void RegionalPolicyAssignmentStartsInProposed()
    {
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("code-279", "title-497");
        Check.Equal(item.Status, RegionalPolicyAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPolicyAssignmentMovesToRejected()
    {
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("code-279", "title-497");
        try
        {
            item.MoveTo(RegionalPolicyAssignmentStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPolicyAssignmentStatus.Rejected, "status");
    }

    public static void RegionalPolicyAssignmentRejectsMovesFromWithdrawn()
    {
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("code-279", "title-497");
        item.Status = RegionalPolicyAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalPolicyAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalPolicyAssignmentStartsInProposed();
        RegionalPolicyAssignmentMovesToRejected();
        RegionalPolicyAssignmentRejectsMovesFromWithdrawn();
    }
}
