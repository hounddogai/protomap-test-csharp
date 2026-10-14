namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyManifestChecks
{
    public static void WarrantyManifestStartsInPlanned()
    {
        var item = WarrantyManifest.NewWarrantyManifest("code-144", "name-572");
        Check.Equal(item.Status, WarrantyManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyManifestMovesToInProgress()
    {
        var item = WarrantyManifest.NewWarrantyManifest("code-144", "name-572");
        try
        {
            item.MoveTo(WarrantyManifestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyManifestStatus.InProgress, "status");
    }

    public static void WarrantyManifestRejectsMovesFromCancelled()
    {
        var item = WarrantyManifest.NewWarrantyManifest("code-144", "name-572");
        item.Status = WarrantyManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WarrantyManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WarrantyManifestStartsInPlanned();
        WarrantyManifestMovesToInProgress();
        WarrantyManifestRejectsMovesFromCancelled();
    }
}
