namespace Acme.Workforce.Absence.Tests.Holiday.Manifest;

using Acme.Workforce.Absence.Holiday.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class HolidayManifestChecks
{
    public static void HolidayManifestStartsInDraft()
    {
        var item = HolidayManifest.NewHolidayManifest("code-850", "name-687");
        Check.Equal(item.Status, HolidayManifestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayManifestMovesToActive()
    {
        var item = HolidayManifest.NewHolidayManifest("code-850", "name-687");
        try
        {
            item.MoveTo(HolidayManifestStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayManifestStatus.Active, "status");
    }

    public static void HolidayManifestRejectsMovesFromClosed()
    {
        var item = HolidayManifest.NewHolidayManifest("code-850", "name-687");
        item.Status = HolidayManifestStatus.Closed;
        Check.Fails(() => item.MoveTo(HolidayManifestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        HolidayManifestStartsInDraft();
        HolidayManifestMovesToActive();
        HolidayManifestRejectsMovesFromClosed();
    }
}
