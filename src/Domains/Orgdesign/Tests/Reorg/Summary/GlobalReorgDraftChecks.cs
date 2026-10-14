namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalReorgDraftChecks
{
    public static void GlobalReorgDraftStartsInIdle()
    {
        var item = GlobalReorgDraft.NewGlobalReorgDraft("code-556", "title-722");
        Check.Equal(item.Status, GlobalReorgDraftStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalReorgDraftMovesToReady()
    {
        var item = GlobalReorgDraft.NewGlobalReorgDraft("code-556", "title-722");
        try
        {
            item.MoveTo(GlobalReorgDraftStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalReorgDraftStatus.Ready, "status");
    }

    public static void GlobalReorgDraftRejectsMovesFromStopped()
    {
        var item = GlobalReorgDraft.NewGlobalReorgDraft("code-556", "title-722");
        item.Status = GlobalReorgDraftStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalReorgDraftStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalReorgDraftStartsInIdle();
        GlobalReorgDraftMovesToReady();
        GlobalReorgDraftRejectsMovesFromStopped();
    }
}
