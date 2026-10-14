namespace Acme.Workforce.Assets.Tests.Laptop.LaptopManifest;

using Acme.Workforce.Assets.Laptop.LaptopManifest;

/// <summary>Checks of laptop manifest code, run by the project's runner.</summary>
public static class ManualLaptopBundleChecks
{
    public static void ManualLaptopBundleStartsInNew()
    {
        var item = ManualLaptopBundle.NewManualLaptopBundle("code-423", "name-939");
        Check.Equal(item.Status, ManualLaptopBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualLaptopBundleMovesToDone()
    {
        var item = ManualLaptopBundle.NewManualLaptopBundle("code-423", "name-939");
        try
        {
            item.MoveTo(ManualLaptopBundleStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualLaptopBundleStatus.Done, "status");
    }

    public static void ManualLaptopBundleRejectsMovesFromFailed()
    {
        var item = ManualLaptopBundle.NewManualLaptopBundle("code-423", "name-939");
        item.Status = ManualLaptopBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualLaptopBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualLaptopBundleStartsInNew();
        ManualLaptopBundleMovesToDone();
        ManualLaptopBundleRejectsMovesFromFailed();
    }
}
