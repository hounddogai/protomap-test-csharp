namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptiveBookingProfileChecks
{
    public static void AdaptiveBookingProfileStartsInIdle()
    {
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("code-618", "label-321");
        Check.Equal(item.Status, AdaptiveBookingProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveBookingProfileMovesToStopped()
    {
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("code-618", "label-321");
        try
        {
            item.MoveTo(AdaptiveBookingProfileStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveBookingProfileStatus.Stopped, "status");
    }

    public static void AdaptiveBookingProfileRejectsMovesFromStopped()
    {
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("code-618", "label-321");
        item.Status = AdaptiveBookingProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveBookingProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveBookingProfileStartsInIdle();
        AdaptiveBookingProfileMovesToStopped();
        AdaptiveBookingProfileRejectsMovesFromStopped();
    }
}
