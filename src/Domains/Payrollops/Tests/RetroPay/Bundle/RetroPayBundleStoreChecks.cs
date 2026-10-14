namespace Acme.Workforce.Payrollops.Tests.RetroPay.Bundle;

using Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RetroPayBundleStoreChecks
{
    public static void MemoryRetroPayBundleStoreFindsWhatItSaved()
    {
        var store = RetroPayBundleStore.NewMemoryRetroPayBundleStore();
        var item = RetroPayBundle.NewRetroPayBundle("code-138", "label-448");
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
        MemoryRetroPayBundleStoreFindsWhatItSaved();
    }
}
