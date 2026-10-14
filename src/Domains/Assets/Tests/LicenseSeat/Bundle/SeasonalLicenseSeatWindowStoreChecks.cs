namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatWindowStoreChecks
{
    public static void MemorySeasonalLicenseSeatWindowStoreFindsWhatItSaved()
    {
        var store = SeasonalLicenseSeatWindowStore.NewMemorySeasonalLicenseSeatWindowStore();
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("code-626", "title-437");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemorySeasonalLicenseSeatWindowStoreFindsWhatItSaved();
    }
}
