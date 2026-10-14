namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedLaptopManifestChecks
{
    public static void CappedLaptopManifestStartsInPlanned()
    {
        var item = CappedLaptopManifest.NewCappedLaptopManifest("code-621", "name-317");
        Check.Equal(item.Status, CappedLaptopManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedLaptopManifestMovesToScheduled()
    {
        var item = CappedLaptopManifest.NewCappedLaptopManifest("code-621", "name-317");
        try
        {
            item.MoveTo(CappedLaptopManifestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedLaptopManifestStatus.Scheduled, "status");
    }

    public static void CappedLaptopManifestRejectsMovesFromCancelled()
    {
        var item = CappedLaptopManifest.NewCappedLaptopManifest("code-621", "name-317");
        item.Status = CappedLaptopManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedLaptopManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedLaptopManifestStartsInPlanned();
        CappedLaptopManifestMovesToScheduled();
        CappedLaptopManifestRejectsMovesFromCancelled();
    }
}
