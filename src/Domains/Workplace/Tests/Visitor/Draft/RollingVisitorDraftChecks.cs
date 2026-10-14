namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingVisitorDraftChecks
{
    public static void RollingVisitorDraftStartsInProposed()
    {
        var item = RollingVisitorDraft.NewRollingVisitorDraft("code-72", "title-686");
        Check.Equal(item.Status, RollingVisitorDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingVisitorDraftMovesToRejected()
    {
        var item = RollingVisitorDraft.NewRollingVisitorDraft("code-72", "title-686");
        try
        {
            item.MoveTo(RollingVisitorDraftStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingVisitorDraftStatus.Rejected, "status");
    }

    public static void RollingVisitorDraftRejectsMovesFromWithdrawn()
    {
        var item = RollingVisitorDraft.NewRollingVisitorDraft("code-72", "title-686");
        item.Status = RollingVisitorDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingVisitorDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingVisitorDraftStartsInProposed();
        RollingVisitorDraftMovesToRejected();
        RollingVisitorDraftRejectsMovesFromWithdrawn();
    }
}
