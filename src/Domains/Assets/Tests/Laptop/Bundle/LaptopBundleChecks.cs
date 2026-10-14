namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopBundleChecks
{
    public static void LaptopBundleStartsInPlanned()
    {
        var item = LaptopBundle.NewLaptopBundle("code-510", "title-454");
        Check.Equal(item.Status, LaptopBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopBundleMovesToInProgress()
    {
        var item = LaptopBundle.NewLaptopBundle("code-510", "title-454");
        try
        {
            item.MoveTo(LaptopBundleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopBundleStatus.InProgress, "status");
    }

    public static void LaptopBundleRejectsMovesFromCancelled()
    {
        var item = LaptopBundle.NewLaptopBundle("code-510", "title-454");
        item.Status = LaptopBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LaptopBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LaptopBundleStartsInPlanned();
        LaptopBundleMovesToInProgress();
        LaptopBundleRejectsMovesFromCancelled();
    }
}
