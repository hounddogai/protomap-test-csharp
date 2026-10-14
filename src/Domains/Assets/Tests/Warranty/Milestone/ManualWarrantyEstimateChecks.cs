namespace Acme.Workforce.Assets.Tests.Warranty.Milestone;

using Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ManualWarrantyEstimateChecks
{
    public static void ManualWarrantyEstimateStartsInIdle()
    {
        var item = ManualWarrantyEstimate.NewManualWarrantyEstimate("code-993", "label-934");
        Check.Equal(item.Status, ManualWarrantyEstimateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualWarrantyEstimateMovesToWarming()
    {
        var item = ManualWarrantyEstimate.NewManualWarrantyEstimate("code-993", "label-934");
        try
        {
            item.MoveTo(ManualWarrantyEstimateStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualWarrantyEstimateStatus.Warming, "status");
    }

    public static void ManualWarrantyEstimateRejectsMovesFromStopped()
    {
        var item = ManualWarrantyEstimate.NewManualWarrantyEstimate("code-993", "label-934");
        item.Status = ManualWarrantyEstimateStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualWarrantyEstimateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualWarrantyEstimateStartsInIdle();
        ManualWarrantyEstimateMovesToWarming();
        ManualWarrantyEstimateRejectsMovesFromStopped();
    }
}
