namespace Acme.Workforce.Assets.Tests.Assignment.Rollup;

using Acme.Workforce.Assets.Assignment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AssignmentDraftChecks
{
    public static void AssignmentDraftStartsInRequested()
    {
        var item = AssignmentDraft.NewAssignmentDraft("code-225", "label-696");
        Check.Equal(item.Status, AssignmentDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentDraftMovesToRevoked()
    {
        var item = AssignmentDraft.NewAssignmentDraft("code-225", "label-696");
        try
        {
            item.MoveTo(AssignmentDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentDraftStatus.Revoked, "status");
    }

    public static void AssignmentDraftRejectsMovesFromExpired()
    {
        var item = AssignmentDraft.NewAssignmentDraft("code-225", "label-696");
        item.Status = AssignmentDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(AssignmentDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AssignmentDraftStartsInRequested();
        AssignmentDraftMovesToRevoked();
        AssignmentDraftRejectsMovesFromExpired();
    }
}
