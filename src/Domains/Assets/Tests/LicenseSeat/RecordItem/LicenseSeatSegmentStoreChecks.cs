namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LicenseSeatSegmentStoreChecks
{
    public static void MemoryLicenseSeatSegmentStoreFindsWhatItSaved()
    {
        var store = LicenseSeatSegmentStore.NewMemoryLicenseSeatSegmentStore();
        var item = LicenseSeatSegment.NewLicenseSeatSegment("code-431", "label-701");
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
        MemoryLicenseSeatSegmentStoreFindsWhatItSaved();
    }
}
