namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class DefaultHeadcountDraftChecks
{
    public static void DefaultHeadcountDraftStartsInNew()
    {
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("code-35", "label-540");
        Check.Equal(item.Status, DefaultHeadcountDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHeadcountDraftMovesToRunning()
    {
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("code-35", "label-540");
        try
        {
            item.MoveTo(DefaultHeadcountDraftStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHeadcountDraftStatus.Running, "status");
    }

    public static void DefaultHeadcountDraftRejectsMovesFromFailed()
    {
        var item = DefaultHeadcountDraft.NewDefaultHeadcountDraft("code-35", "label-540");
        item.Status = DefaultHeadcountDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultHeadcountDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultHeadcountDraftStartsInNew();
        DefaultHeadcountDraftMovesToRunning();
        DefaultHeadcountDraftRejectsMovesFromFailed();
    }
}
