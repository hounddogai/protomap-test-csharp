namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingManifestChecks
{
    public static void OffboardingManifestStartsInPlanned()
    {
        var item = OffboardingManifest.NewOffboardingManifest("code-880", "title-220");
        Check.Equal(item.Status, OffboardingManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingManifestMovesToInProgress()
    {
        var item = OffboardingManifest.NewOffboardingManifest("code-880", "title-220");
        try
        {
            item.MoveTo(OffboardingManifestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingManifestStatus.InProgress, "status");
    }

    public static void OffboardingManifestRejectsMovesFromCancelled()
    {
        var item = OffboardingManifest.NewOffboardingManifest("code-880", "title-220");
        item.Status = OffboardingManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OffboardingManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OffboardingManifestStartsInPlanned();
        OffboardingManifestMovesToInProgress();
        OffboardingManifestRejectsMovesFromCancelled();
    }
}
