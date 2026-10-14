namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultLicenseSeatSummaryStoreChecks
{
    public static void MemoryDefaultLicenseSeatSummaryStoreFindsWhatItSaved()
    {
        var store = DefaultLicenseSeatSummaryStore.NewMemoryDefaultLicenseSeatSummaryStore();
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("code-50", "title-305");
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
        MemoryDefaultLicenseSeatSummaryStoreFindsWhatItSaved();
    }
}
