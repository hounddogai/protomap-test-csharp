namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LenientLaptopMetricChecks
{
    public static void LenientLaptopMetricStartsInDraft()
    {
        var item = LenientLaptopMetric.NewLenientLaptopMetric("code-880", "name-518");
        Check.Equal(item.Status, LenientLaptopMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLaptopMetricMovesToActive()
    {
        var item = LenientLaptopMetric.NewLenientLaptopMetric("code-880", "name-518");
        try
        {
            item.MoveTo(LenientLaptopMetricStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLaptopMetricStatus.Active, "status");
    }

    public static void LenientLaptopMetricRejectsMovesFromClosed()
    {
        var item = LenientLaptopMetric.NewLenientLaptopMetric("code-880", "name-518");
        item.Status = LenientLaptopMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientLaptopMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientLaptopMetricStartsInDraft();
        LenientLaptopMetricMovesToActive();
        LenientLaptopMetricRejectsMovesFromClosed();
    }
}
