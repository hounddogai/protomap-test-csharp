namespace Acme.Workforce.Compensation.Tests.PayBand.Batch;

using Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayBandWindowStoreChecks
{
    public static void MemoryPayBandWindowStoreFindsWhatItSaved()
    {
        var store = PayBandWindowStore.NewMemoryPayBandWindowStore();
        var item = PayBandWindow.NewPayBandWindow("code-186", "title-78");
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
        MemoryPayBandWindowStoreFindsWhatItSaved();
    }
}
