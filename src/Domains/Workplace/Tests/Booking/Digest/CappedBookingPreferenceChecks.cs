namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CappedBookingPreferenceChecks
{
    public static void CappedBookingPreferenceStartsInOpen()
    {
        var item = CappedBookingPreference.NewCappedBookingPreference("code-162", "label-875");
        Check.Equal(item.Status, CappedBookingPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedBookingPreferenceMovesToInReview()
    {
        var item = CappedBookingPreference.NewCappedBookingPreference("code-162", "label-875");
        try
        {
            item.MoveTo(CappedBookingPreferenceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedBookingPreferenceStatus.InReview, "status");
    }

    public static void CappedBookingPreferenceRejectsMovesFromArchived()
    {
        var item = CappedBookingPreference.NewCappedBookingPreference("code-162", "label-875");
        item.Status = CappedBookingPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedBookingPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedBookingPreferenceStartsInOpen();
        CappedBookingPreferenceMovesToInReview();
        CappedBookingPreferenceRejectsMovesFromArchived();
    }
}
