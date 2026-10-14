namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedLicenseSeatBundleStoreChecks
{
    public static void MemoryWeightedLicenseSeatBundleStoreFindsWhatItSaved()
    {
        var store = WeightedLicenseSeatBundleStore.NewMemoryWeightedLicenseSeatBundleStore();
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("code-344", "label-691");
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
        MemoryWeightedLicenseSeatBundleStoreFindsWhatItSaved();
    }
}
