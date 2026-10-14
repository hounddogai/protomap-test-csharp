namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class StrictPayBandDraftStoreChecks
{
    public static void MemoryStrictPayBandDraftStoreFindsWhatItSaved()
    {
        var store = StrictPayBandDraftStore.NewMemoryStrictPayBandDraftStore();
        var item = StrictPayBandDraft.NewStrictPayBandDraft("code-622", "name-946");
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
        MemoryStrictPayBandDraftStoreFindsWhatItSaved();
    }
}
