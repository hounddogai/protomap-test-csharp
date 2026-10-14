namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PayBandGroupStoreChecks
{
    public static void MemoryPayBandGroupStoreFindsWhatItSaved()
    {
        var store = PayBandGroupStore.NewMemoryPayBandGroupStore();
        var item = PayBandGroup.NewPayBandGroup("code-674", "label-609");
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
        MemoryPayBandGroupStoreFindsWhatItSaved();
    }
}
