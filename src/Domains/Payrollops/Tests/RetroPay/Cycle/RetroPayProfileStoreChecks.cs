namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RetroPayProfileStoreChecks
{
    public static void MemoryRetroPayProfileStoreFindsWhatItSaved()
    {
        var store = RetroPayProfileStore.NewMemoryRetroPayProfileStore();
        var item = RetroPayProfile.NewRetroPayProfile("code-334", "title-488");
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
        MemoryRetroPayProfileStoreFindsWhatItSaved();
    }
}
