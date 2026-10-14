namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopCheckpointChecks
{
    public static void LaptopCheckpointStartsInPlanned()
    {
        var item = LaptopCheckpoint.NewLaptopCheckpoint("code-641", "label-4");
        Check.Equal(item.Status, LaptopCheckpointStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopCheckpointMovesToCancelled()
    {
        var item = LaptopCheckpoint.NewLaptopCheckpoint("code-641", "label-4");
        try
        {
            item.MoveTo(LaptopCheckpointStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopCheckpointStatus.Cancelled, "status");
    }

    public static void LaptopCheckpointRejectsMovesFromCancelled()
    {
        var item = LaptopCheckpoint.NewLaptopCheckpoint("code-641", "label-4");
        item.Status = LaptopCheckpointStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LaptopCheckpointStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LaptopCheckpointStartsInPlanned();
        LaptopCheckpointMovesToCancelled();
        LaptopCheckpointRejectsMovesFromCancelled();
    }
}
