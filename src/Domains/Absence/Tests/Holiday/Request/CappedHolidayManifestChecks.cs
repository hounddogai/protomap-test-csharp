namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CappedHolidayManifestChecks
{
    public static void CappedHolidayManifestStartsInDraft()
    {
        var item = CappedHolidayManifest.NewCappedHolidayManifest("code-596", "label-536");
        Check.Equal(item.Status, CappedHolidayManifestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedHolidayManifestMovesToPending()
    {
        var item = CappedHolidayManifest.NewCappedHolidayManifest("code-596", "label-536");
        try
        {
            item.MoveTo(CappedHolidayManifestStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedHolidayManifestStatus.Pending, "status");
    }

    public static void CappedHolidayManifestRejectsMovesFromClosed()
    {
        var item = CappedHolidayManifest.NewCappedHolidayManifest("code-596", "label-536");
        item.Status = CappedHolidayManifestStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedHolidayManifestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedHolidayManifestStartsInDraft();
        CappedHolidayManifestMovesToPending();
        CappedHolidayManifestRejectsMovesFromClosed();
    }
}
