namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RefreshTierChecks
{
    public static void RefreshTierStartsInCreated()
    {
        var item = RefreshTier.NewRefreshTier("code-486", "title-486");
        Check.Equal(item.Status, RefreshTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshTierMovesToPublished()
    {
        var item = RefreshTier.NewRefreshTier("code-486", "title-486");
        try
        {
            item.MoveTo(RefreshTierStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshTierStatus.Published, "status");
    }

    public static void RefreshTierRejectsMovesFromRetired()
    {
        var item = RefreshTier.NewRefreshTier("code-486", "title-486");
        item.Status = RefreshTierStatus.Retired;
        Check.Fails(() => item.MoveTo(RefreshTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        RefreshTierStartsInCreated();
        RefreshTierMovesToPublished();
        RefreshTierRejectsMovesFromRetired();
    }
}
