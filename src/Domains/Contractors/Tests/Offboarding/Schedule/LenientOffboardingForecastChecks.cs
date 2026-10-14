namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientOffboardingForecastChecks
{
    public static void LenientOffboardingForecastStartsInProposed()
    {
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("code-67", "name-379");
        Check.Equal(item.Status, LenientOffboardingForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientOffboardingForecastMovesToRejected()
    {
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("code-67", "name-379");
        try
        {
            item.MoveTo(LenientOffboardingForecastStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientOffboardingForecastStatus.Rejected, "status");
    }

    public static void LenientOffboardingForecastRejectsMovesFromWithdrawn()
    {
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("code-67", "name-379");
        item.Status = LenientOffboardingForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientOffboardingForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientOffboardingForecastStartsInProposed();
        LenientOffboardingForecastMovesToRejected();
        LenientOffboardingForecastRejectsMovesFromWithdrawn();
    }
}
