namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveLaptopCheckpointChecks
{
    public static void AdaptiveLaptopCheckpointStartsInCreated()
    {
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("code-968", "label-883");
        Check.Equal(item.Status, AdaptiveLaptopCheckpointStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveLaptopCheckpointMovesToValidated()
    {
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("code-968", "label-883");
        try
        {
            item.MoveTo(AdaptiveLaptopCheckpointStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveLaptopCheckpointStatus.Validated, "status");
    }

    public static void AdaptiveLaptopCheckpointRejectsMovesFromRetired()
    {
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("code-968", "label-883");
        item.Status = AdaptiveLaptopCheckpointStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveLaptopCheckpointStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveLaptopCheckpointStartsInCreated();
        AdaptiveLaptopCheckpointMovesToValidated();
        AdaptiveLaptopCheckpointRejectsMovesFromRetired();
    }
}
