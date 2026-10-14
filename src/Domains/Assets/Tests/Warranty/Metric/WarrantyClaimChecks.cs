namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyClaimChecks
{
    public static void WarrantyClaimStartsInOpen()
    {
        var item = WarrantyClaim.NewWarrantyClaim("code-400", "name-178");
        Check.Equal(item.Status, WarrantyClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyClaimMovesToResolved()
    {
        var item = WarrantyClaim.NewWarrantyClaim("code-400", "name-178");
        try
        {
            item.MoveTo(WarrantyClaimStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyClaimStatus.Resolved, "status");
    }

    public static void WarrantyClaimRejectsMovesFromArchived()
    {
        var item = WarrantyClaim.NewWarrantyClaim("code-400", "name-178");
        item.Status = WarrantyClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(WarrantyClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        WarrantyClaimStartsInOpen();
        WarrantyClaimMovesToResolved();
        WarrantyClaimRejectsMovesFromArchived();
    }
}
