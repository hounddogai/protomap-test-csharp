namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopMetricChecks
{
    public static void LaptopMetricStartsInDraft()
    {
        var item = LaptopMetric.NewLaptopMetric("code-46", "label-739");
        Check.Equal(item.Status, LaptopMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopMetricMovesToPending()
    {
        var item = LaptopMetric.NewLaptopMetric("code-46", "label-739");
        try
        {
            item.MoveTo(LaptopMetricStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopMetricStatus.Pending, "status");
    }

    public static void LaptopMetricRejectsMovesFromClosed()
    {
        var item = LaptopMetric.NewLaptopMetric("code-46", "label-739");
        item.Status = LaptopMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(LaptopMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LaptopMetricStartsInDraft();
        LaptopMetricMovesToPending();
        LaptopMetricRejectsMovesFromClosed();
    }
}
