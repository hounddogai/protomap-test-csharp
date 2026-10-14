namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveLaptopManifestChecks
{
    public static void AdaptiveLaptopManifestStartsInRequested()
    {
        var item = AdaptiveLaptopManifest.NewAdaptiveLaptopManifest("code-914", "title-349");
        Check.Equal(item.Status, AdaptiveLaptopManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveLaptopManifestMovesToExpired()
    {
        var item = AdaptiveLaptopManifest.NewAdaptiveLaptopManifest("code-914", "title-349");
        try
        {
            item.MoveTo(AdaptiveLaptopManifestStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveLaptopManifestStatus.Expired, "status");
    }

    public static void AdaptiveLaptopManifestRejectsMovesFromExpired()
    {
        var item = AdaptiveLaptopManifest.NewAdaptiveLaptopManifest("code-914", "title-349");
        item.Status = AdaptiveLaptopManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveLaptopManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveLaptopManifestStartsInRequested();
        AdaptiveLaptopManifestMovesToExpired();
        AdaptiveLaptopManifestRejectsMovesFromExpired();
    }
}
