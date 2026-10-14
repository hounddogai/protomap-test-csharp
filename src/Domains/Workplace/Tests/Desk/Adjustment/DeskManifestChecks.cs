namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskManifestChecks
{
    public static void DeskManifestStartsInRequested()
    {
        var item = DeskManifest.NewDeskManifest("code-49", "name-575");
        Check.Equal(item.Status, DeskManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskManifestMovesToGranted()
    {
        var item = DeskManifest.NewDeskManifest("code-49", "name-575");
        try
        {
            item.MoveTo(DeskManifestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskManifestStatus.Granted, "status");
    }

    public static void DeskManifestRejectsMovesFromExpired()
    {
        var item = DeskManifest.NewDeskManifest("code-49", "name-575");
        item.Status = DeskManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(DeskManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DeskManifestStartsInRequested();
        DeskManifestMovesToGranted();
        DeskManifestRejectsMovesFromExpired();
    }
}
