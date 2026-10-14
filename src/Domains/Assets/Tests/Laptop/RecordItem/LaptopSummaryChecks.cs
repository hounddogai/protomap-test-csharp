namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopSummaryChecks
{
    public static void LaptopSummaryStartsInIdle()
    {
        var item = LaptopSummary.NewLaptopSummary("code-336", "title-906");
        Check.Equal(item.Status, LaptopSummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopSummaryMovesToReady()
    {
        var item = LaptopSummary.NewLaptopSummary("code-336", "title-906");
        try
        {
            item.MoveTo(LaptopSummaryStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopSummaryStatus.Ready, "status");
    }

    public static void LaptopSummaryRejectsMovesFromStopped()
    {
        var item = LaptopSummary.NewLaptopSummary("code-336", "title-906");
        item.Status = LaptopSummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(LaptopSummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LaptopSummaryStartsInIdle();
        LaptopSummaryMovesToReady();
        LaptopSummaryRejectsMovesFromStopped();
    }
}
