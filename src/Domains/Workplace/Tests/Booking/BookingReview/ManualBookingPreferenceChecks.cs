namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class ManualBookingPreferenceChecks
{
    public static void ManualBookingPreferenceStartsInCreated()
    {
        var item = ManualBookingPreference.NewManualBookingPreference("code-572", "title-166");
        Check.Equal(item.Status, ManualBookingPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBookingPreferenceMovesToValidated()
    {
        var item = ManualBookingPreference.NewManualBookingPreference("code-572", "title-166");
        try
        {
            item.MoveTo(ManualBookingPreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBookingPreferenceStatus.Validated, "status");
    }

    public static void ManualBookingPreferenceRejectsMovesFromRetired()
    {
        var item = ManualBookingPreference.NewManualBookingPreference("code-572", "title-166");
        item.Status = ManualBookingPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualBookingPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualBookingPreferenceStartsInCreated();
        ManualBookingPreferenceMovesToValidated();
        ManualBookingPreferenceRejectsMovesFromRetired();
    }
}
