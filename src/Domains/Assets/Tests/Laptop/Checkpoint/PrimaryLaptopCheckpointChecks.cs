namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PrimaryLaptopCheckpointChecks
{
    public static void PrimaryLaptopCheckpointStartsInCreated()
    {
        var item = PrimaryLaptopCheckpoint.NewPrimaryLaptopCheckpoint("code-642", "label-763");
        Check.Equal(item.Status, PrimaryLaptopCheckpointStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryLaptopCheckpointMovesToPublished()
    {
        var item = PrimaryLaptopCheckpoint.NewPrimaryLaptopCheckpoint("code-642", "label-763");
        try
        {
            item.MoveTo(PrimaryLaptopCheckpointStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryLaptopCheckpointStatus.Published, "status");
    }

    public static void PrimaryLaptopCheckpointRejectsMovesFromRetired()
    {
        var item = PrimaryLaptopCheckpoint.NewPrimaryLaptopCheckpoint("code-642", "label-763");
        item.Status = PrimaryLaptopCheckpointStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryLaptopCheckpointStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryLaptopCheckpointStartsInCreated();
        PrimaryLaptopCheckpointMovesToPublished();
        PrimaryLaptopCheckpointRejectsMovesFromRetired();
    }
}
