namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingBundleChecks
{
    public static void OffboardingBundleStartsInRequested()
    {
        var item = OffboardingBundle.NewOffboardingBundle("code-129", "name-918");
        Check.Equal(item.Status, OffboardingBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingBundleMovesToGranted()
    {
        var item = OffboardingBundle.NewOffboardingBundle("code-129", "name-918");
        try
        {
            item.MoveTo(OffboardingBundleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingBundleStatus.Granted, "status");
    }

    public static void OffboardingBundleRejectsMovesFromExpired()
    {
        var item = OffboardingBundle.NewOffboardingBundle("code-129", "name-918");
        item.Status = OffboardingBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(OffboardingBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        OffboardingBundleStartsInRequested();
        OffboardingBundleMovesToGranted();
        OffboardingBundleRejectsMovesFromExpired();
    }
}
