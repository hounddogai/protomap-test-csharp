namespace Acme.Workforce.Assets.Tests.Assignment.Profile;

using Acme.Workforce.Assets.Assignment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssignmentProfileChecks
{
    public static void AssignmentProfileStartsInProposed()
    {
        var item = AssignmentProfile.NewAssignmentProfile("code-542", "name-81");
        Check.Equal(item.Status, AssignmentProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentProfileMovesToApproved()
    {
        var item = AssignmentProfile.NewAssignmentProfile("code-542", "name-81");
        try
        {
            item.MoveTo(AssignmentProfileStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentProfileStatus.Approved, "status");
    }

    public static void AssignmentProfileRejectsMovesFromWithdrawn()
    {
        var item = AssignmentProfile.NewAssignmentProfile("code-542", "name-81");
        item.Status = AssignmentProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssignmentProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssignmentProfileStartsInProposed();
        AssignmentProfileMovesToApproved();
        AssignmentProfileRejectsMovesFromWithdrawn();
    }
}
