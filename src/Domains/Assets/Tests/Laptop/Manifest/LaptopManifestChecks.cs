namespace Acme.Workforce.Assets.Tests.Laptop.Manifest;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LaptopManifestChecks
{
    public static void LaptopManifestStartsInPlanned()
    {
        var item = LaptopManifest.NewLaptopManifest("code-76", "title-288");
        Check.Equal(item.Status, LaptopManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopManifestMovesToInProgress()
    {
        var item = LaptopManifest.NewLaptopManifest("code-76", "title-288");
        try
        {
            item.MoveTo(LaptopManifestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopManifestStatus.InProgress, "status");
    }

    public static void LaptopManifestRejectsMovesFromCancelled()
    {
        var item = LaptopManifest.NewLaptopManifest("code-76", "title-288");
        item.Status = LaptopManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LaptopManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LaptopManifestStartsInPlanned();
        LaptopManifestMovesToInProgress();
        LaptopManifestRejectsMovesFromCancelled();
    }
}
