namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantySegmentChecks
{
    public static void WarrantySegmentStartsInIdle()
    {
        var item = WarrantySegment.NewWarrantySegment("code-584", "label-712");
        Check.Equal(item.Status, WarrantySegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantySegmentMovesToDraining()
    {
        var item = WarrantySegment.NewWarrantySegment("code-584", "label-712");
        try
        {
            item.MoveTo(WarrantySegmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantySegmentStatus.Draining, "status");
    }

    public static void WarrantySegmentRejectsMovesFromStopped()
    {
        var item = WarrantySegment.NewWarrantySegment("code-584", "label-712");
        item.Status = WarrantySegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(WarrantySegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WarrantySegmentStartsInIdle();
        WarrantySegmentMovesToDraining();
        WarrantySegmentRejectsMovesFromStopped();
    }
}
