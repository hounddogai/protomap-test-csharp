namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RollingLaptopBaselineChecks
{
    public static void RollingLaptopBaselineStartsInOpen()
    {
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("code-124", "name-970");
        Check.Equal(item.Status, RollingLaptopBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingLaptopBaselineMovesToResolved()
    {
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("code-124", "name-970");
        try
        {
            item.MoveTo(RollingLaptopBaselineStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingLaptopBaselineStatus.Resolved, "status");
    }

    public static void RollingLaptopBaselineRejectsMovesFromArchived()
    {
        var item = RollingLaptopBaseline.NewRollingLaptopBaseline("code-124", "name-970");
        item.Status = RollingLaptopBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingLaptopBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingLaptopBaselineStartsInOpen();
        RollingLaptopBaselineMovesToResolved();
        RollingLaptopBaselineRejectsMovesFromArchived();
    }
}
