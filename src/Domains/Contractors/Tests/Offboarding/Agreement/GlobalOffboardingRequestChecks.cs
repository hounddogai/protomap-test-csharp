namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GlobalOffboardingRequestChecks
{
    public static void GlobalOffboardingRequestStartsInRequested()
    {
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("code-387", "label-213");
        Check.Equal(item.Status, GlobalOffboardingRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalOffboardingRequestMovesToGranted()
    {
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("code-387", "label-213");
        try
        {
            item.MoveTo(GlobalOffboardingRequestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalOffboardingRequestStatus.Granted, "status");
    }

    public static void GlobalOffboardingRequestRejectsMovesFromExpired()
    {
        var item = GlobalOffboardingRequest.NewGlobalOffboardingRequest("code-387", "label-213");
        item.Status = GlobalOffboardingRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalOffboardingRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalOffboardingRequestStartsInRequested();
        GlobalOffboardingRequestMovesToGranted();
        GlobalOffboardingRequestRejectsMovesFromExpired();
    }
}
