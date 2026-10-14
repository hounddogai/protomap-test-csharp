namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayEntryStoreChecks
{
    public static void MemoryRetroPayEntryStoreFindsWhatItSaved()
    {
        var store = RetroPayEntryStore.NewMemoryRetroPayEntryStore();
        var item = RetroPayEntry.NewRetroPayEntry("code-729", "name-386");
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
        MemoryRetroPayEntryStoreFindsWhatItSaved();
    }
}
