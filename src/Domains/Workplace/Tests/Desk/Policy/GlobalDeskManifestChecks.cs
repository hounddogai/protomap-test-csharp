namespace Acme.Workforce.Workplace.Tests.Desk.Policy;

using Acme.Workforce.Workplace.Desk.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalDeskManifestChecks
{
    public static void GlobalDeskManifestStartsInOpen()
    {
        var item = GlobalDeskManifest.NewGlobalDeskManifest("code-490", "title-270");
        Check.Equal(item.Status, GlobalDeskManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalDeskManifestMovesToResolved()
    {
        var item = GlobalDeskManifest.NewGlobalDeskManifest("code-490", "title-270");
        try
        {
            item.MoveTo(GlobalDeskManifestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalDeskManifestStatus.Resolved, "status");
    }

    public static void GlobalDeskManifestRejectsMovesFromArchived()
    {
        var item = GlobalDeskManifest.NewGlobalDeskManifest("code-490", "title-270");
        item.Status = GlobalDeskManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalDeskManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalDeskManifestStartsInOpen();
        GlobalDeskManifestMovesToResolved();
        GlobalDeskManifestRejectsMovesFromArchived();
    }
}
