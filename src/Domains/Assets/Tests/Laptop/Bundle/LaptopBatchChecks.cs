namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopBatchChecks
{
    public static void LaptopBatchStartsInPlanned()
    {
        var item = LaptopBatch.NewLaptopBatch("code-885", "name-297");
        Check.Equal(item.Status, LaptopBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopBatchMovesToScheduled()
    {
        var item = LaptopBatch.NewLaptopBatch("code-885", "name-297");
        try
        {
            item.MoveTo(LaptopBatchStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopBatchStatus.Scheduled, "status");
    }

    public static void LaptopBatchRejectsMovesFromCancelled()
    {
        var item = LaptopBatch.NewLaptopBatch("code-885", "name-297");
        item.Status = LaptopBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LaptopBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LaptopBatchStartsInPlanned();
        LaptopBatchMovesToScheduled();
        LaptopBatchRejectsMovesFromCancelled();
    }
}
