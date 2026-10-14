namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentClaimChecks
{
    public static void AssignmentClaimStartsInDraft()
    {
        var item = AssignmentClaim.NewAssignmentClaim("code-356", "name-308");
        Check.Equal(item.Status, AssignmentClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentClaimMovesToPending()
    {
        var item = AssignmentClaim.NewAssignmentClaim("code-356", "name-308");
        try
        {
            item.MoveTo(AssignmentClaimStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentClaimStatus.Pending, "status");
    }

    public static void AssignmentClaimRejectsMovesFromClosed()
    {
        var item = AssignmentClaim.NewAssignmentClaim("code-356", "name-308");
        item.Status = AssignmentClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(AssignmentClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AssignmentClaimStartsInDraft();
        AssignmentClaimMovesToPending();
        AssignmentClaimRejectsMovesFromClosed();
    }
}
