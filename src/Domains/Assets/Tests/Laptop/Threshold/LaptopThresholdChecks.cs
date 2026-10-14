namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LaptopThresholdChecks
{
    public static void LaptopThresholdStartsInProposed()
    {
        var item = LaptopThreshold.NewLaptopThreshold("code-900", "label-827");
        Check.Equal(item.Status, LaptopThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopThresholdMovesToRejected()
    {
        var item = LaptopThreshold.NewLaptopThreshold("code-900", "label-827");
        try
        {
            item.MoveTo(LaptopThresholdStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopThresholdStatus.Rejected, "status");
    }

    public static void LaptopThresholdRejectsMovesFromWithdrawn()
    {
        var item = LaptopThreshold.NewLaptopThreshold("code-900", "label-827");
        item.Status = LaptopThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LaptopThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LaptopThresholdStartsInProposed();
        LaptopThresholdMovesToRejected();
        LaptopThresholdRejectsMovesFromWithdrawn();
    }
}
