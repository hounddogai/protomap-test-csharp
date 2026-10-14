namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyProfileChecks
{
    public static void SeasonalWarrantyProfileStartsInRequested()
    {
        var item = SeasonalWarrantyProfile.NewSeasonalWarrantyProfile("code-620", "title-115");
        Check.Equal(item.Status, SeasonalWarrantyProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalWarrantyProfileMovesToGranted()
    {
        var item = SeasonalWarrantyProfile.NewSeasonalWarrantyProfile("code-620", "title-115");
        try
        {
            item.MoveTo(SeasonalWarrantyProfileStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalWarrantyProfileStatus.Granted, "status");
    }

    public static void SeasonalWarrantyProfileRejectsMovesFromExpired()
    {
        var item = SeasonalWarrantyProfile.NewSeasonalWarrantyProfile("code-620", "title-115");
        item.Status = SeasonalWarrantyProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalWarrantyProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalWarrantyProfileStartsInRequested();
        SeasonalWarrantyProfileMovesToGranted();
        SeasonalWarrantyProfileRejectsMovesFromExpired();
    }
}
