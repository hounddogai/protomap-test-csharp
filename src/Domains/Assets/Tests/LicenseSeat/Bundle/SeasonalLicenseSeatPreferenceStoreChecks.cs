namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatPreferenceStoreChecks
{
    public static void MemorySeasonalLicenseSeatPreferenceStoreFindsWhatItSaved()
    {
        var store = SeasonalLicenseSeatPreferenceStore.NewMemorySeasonalLicenseSeatPreferenceStore();
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("code-435", "name-136");
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
        MemorySeasonalLicenseSeatPreferenceStoreFindsWhatItSaved();
    }
}
