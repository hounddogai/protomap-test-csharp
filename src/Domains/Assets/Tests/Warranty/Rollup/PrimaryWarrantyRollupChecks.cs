namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryWarrantyRollupChecks
{
    public static void PrimaryWarrantyRollupStartsInOpen()
    {
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("code-977", "title-133");
        Check.Equal(item.Status, PrimaryWarrantyRollupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryWarrantyRollupMovesToInReview()
    {
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("code-977", "title-133");
        try
        {
            item.MoveTo(PrimaryWarrantyRollupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryWarrantyRollupStatus.InReview, "status");
    }

    public static void PrimaryWarrantyRollupRejectsMovesFromArchived()
    {
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("code-977", "title-133");
        item.Status = PrimaryWarrantyRollupStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryWarrantyRollupStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryWarrantyRollupStartsInOpen();
        PrimaryWarrantyRollupMovesToInReview();
        PrimaryWarrantyRollupRejectsMovesFromArchived();
    }
}
