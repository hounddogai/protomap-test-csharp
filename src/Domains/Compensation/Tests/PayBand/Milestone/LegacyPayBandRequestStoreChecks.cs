namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LegacyPayBandRequestStoreChecks
{
    public static void MemoryLegacyPayBandRequestStoreFindsWhatItSaved()
    {
        var store = LegacyPayBandRequestStore.NewMemoryLegacyPayBandRequestStore();
        var item = LegacyPayBandRequest.NewLegacyPayBandRequest("code-601", "name-673");
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
        MemoryLegacyPayBandRequestStoreFindsWhatItSaved();
    }
}
