namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredWarrantyManifestChecks
{
    public static void TieredWarrantyManifestStartsInProposed()
    {
        var item = TieredWarrantyManifest.NewTieredWarrantyManifest("code-849", "name-234");
        Check.Equal(item.Status, TieredWarrantyManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredWarrantyManifestMovesToApproved()
    {
        var item = TieredWarrantyManifest.NewTieredWarrantyManifest("code-849", "name-234");
        try
        {
            item.MoveTo(TieredWarrantyManifestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredWarrantyManifestStatus.Approved, "status");
    }

    public static void TieredWarrantyManifestRejectsMovesFromWithdrawn()
    {
        var item = TieredWarrantyManifest.NewTieredWarrantyManifest("code-849", "name-234");
        item.Status = TieredWarrantyManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredWarrantyManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredWarrantyManifestStartsInProposed();
        TieredWarrantyManifestMovesToApproved();
        TieredWarrantyManifestRejectsMovesFromWithdrawn();
    }
}
