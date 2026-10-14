namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RetroPayOverrideStoreChecks
{
    public static void MemoryRetroPayOverrideStoreFindsWhatItSaved()
    {
        var store = RetroPayOverrideStore.NewMemoryRetroPayOverrideStore();
        var item = RetroPayOverride.NewRetroPayOverride("code-279", "label-858");
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
        MemoryRetroPayOverrideStoreFindsWhatItSaved();
    }
}
