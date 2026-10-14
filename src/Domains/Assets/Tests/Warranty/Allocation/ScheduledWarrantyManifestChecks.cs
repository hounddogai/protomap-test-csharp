namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ScheduledWarrantyManifestChecks
{
    public static void ScheduledWarrantyManifestStartsInProposed()
    {
        var item = ScheduledWarrantyManifest.NewScheduledWarrantyManifest("code-999", "label-483");
        Check.Equal(item.Status, ScheduledWarrantyManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledWarrantyManifestMovesToRejected()
    {
        var item = ScheduledWarrantyManifest.NewScheduledWarrantyManifest("code-999", "label-483");
        try
        {
            item.MoveTo(ScheduledWarrantyManifestStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledWarrantyManifestStatus.Rejected, "status");
    }

    public static void ScheduledWarrantyManifestRejectsMovesFromWithdrawn()
    {
        var item = ScheduledWarrantyManifest.NewScheduledWarrantyManifest("code-999", "label-483");
        item.Status = ScheduledWarrantyManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledWarrantyManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledWarrantyManifestStartsInProposed();
        ScheduledWarrantyManifestMovesToRejected();
        ScheduledWarrantyManifestRejectsMovesFromWithdrawn();
    }
}
