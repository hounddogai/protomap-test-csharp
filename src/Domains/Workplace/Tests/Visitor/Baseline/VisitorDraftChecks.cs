namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class VisitorDraftChecks
{
    public static void VisitorDraftStartsInRequested()
    {
        var item = VisitorDraft.NewVisitorDraft("code-892", "title-877");
        Check.Equal(item.Status, VisitorDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorDraftMovesToExpired()
    {
        var item = VisitorDraft.NewVisitorDraft("code-892", "title-877");
        try
        {
            item.MoveTo(VisitorDraftStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorDraftStatus.Expired, "status");
    }

    public static void VisitorDraftRejectsMovesFromExpired()
    {
        var item = VisitorDraft.NewVisitorDraft("code-892", "title-877");
        item.Status = VisitorDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorDraftStartsInRequested();
        VisitorDraftMovesToExpired();
        VisitorDraftRejectsMovesFromExpired();
    }
}
