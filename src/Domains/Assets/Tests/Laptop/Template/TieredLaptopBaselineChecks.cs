namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredLaptopBaselineChecks
{
    public static void TieredLaptopBaselineStartsInIdle()
    {
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("code-902", "title-475");
        Check.Equal(item.Status, TieredLaptopBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredLaptopBaselineMovesToWarming()
    {
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("code-902", "title-475");
        try
        {
            item.MoveTo(TieredLaptopBaselineStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredLaptopBaselineStatus.Warming, "status");
    }

    public static void TieredLaptopBaselineRejectsMovesFromStopped()
    {
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("code-902", "title-475");
        item.Status = TieredLaptopBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredLaptopBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredLaptopBaselineStartsInIdle();
        TieredLaptopBaselineMovesToWarming();
        TieredLaptopBaselineRejectsMovesFromStopped();
    }
}
