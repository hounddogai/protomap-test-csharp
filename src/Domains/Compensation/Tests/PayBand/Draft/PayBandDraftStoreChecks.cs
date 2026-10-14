namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandDraftStoreChecks
{
    public static void MemoryPayBandDraftStoreFindsWhatItSaved()
    {
        var store = PayBandDraftStore.NewMemoryPayBandDraftStore();
        var item = PayBandDraft.NewPayBandDraft("code-598", "name-624");
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
        MemoryPayBandDraftStoreFindsWhatItSaved();
    }
}
