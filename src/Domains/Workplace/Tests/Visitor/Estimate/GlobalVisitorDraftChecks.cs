namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalVisitorDraftChecks
{
    public static void GlobalVisitorDraftStartsInOpen()
    {
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("code-261", "title-965");
        Check.Equal(item.Status, GlobalVisitorDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalVisitorDraftMovesToInReview()
    {
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("code-261", "title-965");
        try
        {
            item.MoveTo(GlobalVisitorDraftStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalVisitorDraftStatus.InReview, "status");
    }

    public static void GlobalVisitorDraftRejectsMovesFromArchived()
    {
        var item = GlobalVisitorDraft.NewGlobalVisitorDraft("code-261", "title-965");
        item.Status = GlobalVisitorDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalVisitorDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalVisitorDraftStartsInOpen();
        GlobalVisitorDraftMovesToInReview();
        GlobalVisitorDraftRejectsMovesFromArchived();
    }
}
