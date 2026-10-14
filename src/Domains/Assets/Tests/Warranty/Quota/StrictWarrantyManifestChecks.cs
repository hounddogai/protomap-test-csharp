namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictWarrantyManifestChecks
{
    public static void StrictWarrantyManifestStartsInIdle()
    {
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("code-367", "title-226");
        Check.Equal(item.Status, StrictWarrantyManifestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictWarrantyManifestMovesToWarming()
    {
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("code-367", "title-226");
        try
        {
            item.MoveTo(StrictWarrantyManifestStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictWarrantyManifestStatus.Warming, "status");
    }

    public static void StrictWarrantyManifestRejectsMovesFromStopped()
    {
        var item = StrictWarrantyManifest.NewStrictWarrantyManifest("code-367", "title-226");
        item.Status = StrictWarrantyManifestStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictWarrantyManifestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictWarrantyManifestStartsInIdle();
        StrictWarrantyManifestMovesToWarming();
        StrictWarrantyManifestRejectsMovesFromStopped();
    }
}
