namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingThresholdChecks
{
    public static void OffboardingThresholdStartsInRequested()
    {
        var item = OffboardingThreshold.NewOffboardingThreshold("code-193", "title-959");
        Check.Equal(item.Status, OffboardingThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingThresholdMovesToRevoked()
    {
        var item = OffboardingThreshold.NewOffboardingThreshold("code-193", "title-959");
        try
        {
            item.MoveTo(OffboardingThresholdStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingThresholdStatus.Revoked, "status");
    }

    public static void OffboardingThresholdRejectsMovesFromExpired()
    {
        var item = OffboardingThreshold.NewOffboardingThreshold("code-193", "title-959");
        item.Status = OffboardingThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(OffboardingThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        OffboardingThresholdStartsInRequested();
        OffboardingThresholdMovesToRevoked();
        OffboardingThresholdRejectsMovesFromExpired();
    }
}
