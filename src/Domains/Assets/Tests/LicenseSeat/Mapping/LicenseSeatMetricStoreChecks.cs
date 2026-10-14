namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMetricStoreChecks
{
    public static void MemoryLicenseSeatMetricStoreFindsWhatItSaved()
    {
        var store = LicenseSeatMetricStore.NewMemoryLicenseSeatMetricStore();
        var item = LicenseSeatMetric.NewLicenseSeatMetric("code-417", "label-53");
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
        MemoryLicenseSeatMetricStoreFindsWhatItSaved();
    }
}
