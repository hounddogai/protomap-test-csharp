namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalOffboardingRequestChecks
{
    public static void SeasonalOffboardingRequestStartsInRequested()
    {
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("code-441", "label-613");
        Check.Equal(item.Status, SeasonalOffboardingRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalOffboardingRequestMovesToRevoked()
    {
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("code-441", "label-613");
        try
        {
            item.MoveTo(SeasonalOffboardingRequestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalOffboardingRequestStatus.Revoked, "status");
    }

    public static void SeasonalOffboardingRequestRejectsMovesFromExpired()
    {
        var item = SeasonalOffboardingRequest.NewSeasonalOffboardingRequest("code-441", "label-613");
        item.Status = SeasonalOffboardingRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalOffboardingRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalOffboardingRequestStartsInRequested();
        SeasonalOffboardingRequestMovesToRevoked();
        SeasonalOffboardingRequestRejectsMovesFromExpired();
    }
}
