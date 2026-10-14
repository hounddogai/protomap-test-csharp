namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class HolidayRequestChecks
{
    public static void HolidayRequestStartsInPlanned()
    {
        var item = HolidayRequest.NewHolidayRequest("code-964", "name-882");
        Check.Equal(item.Status, HolidayRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayRequestMovesToCancelled()
    {
        var item = HolidayRequest.NewHolidayRequest("code-964", "name-882");
        try
        {
            item.MoveTo(HolidayRequestStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayRequestStatus.Cancelled, "status");
    }

    public static void HolidayRequestRejectsMovesFromCancelled()
    {
        var item = HolidayRequest.NewHolidayRequest("code-964", "name-882");
        item.Status = HolidayRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HolidayRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HolidayRequestStartsInPlanned();
        HolidayRequestMovesToCancelled();
        HolidayRequestRejectsMovesFromCancelled();
    }
}
