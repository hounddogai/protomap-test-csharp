namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DeskDraftChecks
{
    public static void DeskDraftStartsInIdle()
    {
        var item = DeskDraft.NewDeskDraft("code-218", "title-705");
        Check.Equal(item.Status, DeskDraftStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskDraftMovesToWarming()
    {
        var item = DeskDraft.NewDeskDraft("code-218", "title-705");
        try
        {
            item.MoveTo(DeskDraftStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskDraftStatus.Warming, "status");
    }

    public static void DeskDraftRejectsMovesFromStopped()
    {
        var item = DeskDraft.NewDeskDraft("code-218", "title-705");
        item.Status = DeskDraftStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeskDraftStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeskDraftStartsInIdle();
        DeskDraftMovesToWarming();
        DeskDraftRejectsMovesFromStopped();
    }
}
