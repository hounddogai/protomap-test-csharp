namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class HolidayDraftChecks
{
    public static void HolidayDraftStartsInRequested()
    {
        var item = HolidayDraft.NewHolidayDraft("code-181", "title-41");
        Check.Equal(item.Status, HolidayDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayDraftMovesToRevoked()
    {
        var item = HolidayDraft.NewHolidayDraft("code-181", "title-41");
        try
        {
            item.MoveTo(HolidayDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayDraftStatus.Revoked, "status");
    }

    public static void HolidayDraftRejectsMovesFromExpired()
    {
        var item = HolidayDraft.NewHolidayDraft("code-181", "title-41");
        item.Status = HolidayDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(HolidayDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HolidayDraftStartsInRequested();
        HolidayDraftMovesToRevoked();
        HolidayDraftRejectsMovesFromExpired();
    }
}
