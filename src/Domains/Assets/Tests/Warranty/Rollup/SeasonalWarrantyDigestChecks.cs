namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyDigestChecks
{
    public static void SeasonalWarrantyDigestStartsInCreated()
    {
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("code-946", "title-206");
        Check.Equal(item.Status, SeasonalWarrantyDigestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalWarrantyDigestMovesToPublished()
    {
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("code-946", "title-206");
        try
        {
            item.MoveTo(SeasonalWarrantyDigestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalWarrantyDigestStatus.Published, "status");
    }

    public static void SeasonalWarrantyDigestRejectsMovesFromRetired()
    {
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("code-946", "title-206");
        item.Status = SeasonalWarrantyDigestStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalWarrantyDigestStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalWarrantyDigestStartsInCreated();
        SeasonalWarrantyDigestMovesToPublished();
        SeasonalWarrantyDigestRejectsMovesFromRetired();
    }
}
