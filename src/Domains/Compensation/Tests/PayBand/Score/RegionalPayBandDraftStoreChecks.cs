namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RegionalPayBandDraftStoreChecks
{
    public static void MemoryRegionalPayBandDraftStoreFindsWhatItSaved()
    {
        var store = RegionalPayBandDraftStore.NewMemoryRegionalPayBandDraftStore();
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("code-971", "title-999");
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
        MemoryRegionalPayBandDraftStoreFindsWhatItSaved();
    }
}
