namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyEstimateChecks
{
    public static void WarrantyEstimateStartsInCreated()
    {
        var item = WarrantyEstimate.NewWarrantyEstimate("code-714", "label-497");
        Check.Equal(item.Status, WarrantyEstimateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyEstimateMovesToPublished()
    {
        var item = WarrantyEstimate.NewWarrantyEstimate("code-714", "label-497");
        try
        {
            item.MoveTo(WarrantyEstimateStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyEstimateStatus.Published, "status");
    }

    public static void WarrantyEstimateRejectsMovesFromRetired()
    {
        var item = WarrantyEstimate.NewWarrantyEstimate("code-714", "label-497");
        item.Status = WarrantyEstimateStatus.Retired;
        Check.Fails(() => item.MoveTo(WarrantyEstimateStatus.Created), "move");
    }

    public static void RunAll()
    {
        WarrantyEstimateStartsInCreated();
        WarrantyEstimateMovesToPublished();
        WarrantyEstimateRejectsMovesFromRetired();
    }
}
