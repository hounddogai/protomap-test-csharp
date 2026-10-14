namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopMilestoneChecks
{
    public static void LaptopMilestoneStartsInPlanned()
    {
        var item = LaptopMilestone.NewLaptopMilestone("code-794", "title-76");
        Check.Equal(item.Status, LaptopMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopMilestoneMovesToInProgress()
    {
        var item = LaptopMilestone.NewLaptopMilestone("code-794", "title-76");
        try
        {
            item.MoveTo(LaptopMilestoneStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopMilestoneStatus.InProgress, "status");
    }

    public static void LaptopMilestoneRejectsMovesFromCancelled()
    {
        var item = LaptopMilestone.NewLaptopMilestone("code-794", "title-76");
        item.Status = LaptopMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LaptopMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LaptopMilestoneStartsInPlanned();
        LaptopMilestoneMovesToInProgress();
        LaptopMilestoneRejectsMovesFromCancelled();
    }
}
